using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<User> userManager,
            ITokenService tokenService,
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent)
        {
            _logger.LogInformation("Intento de login para usuario: {Username} desde IP: {Ip}", request.Username, ipAddress);

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Login fallido para {User} desde IP {Ip}", request.Username, ipAddress);

                // Si el usuario existe, incrementar intentos fallidos
                if (user != null)
                {
                    user.IncrementFailedLoginAttempts();
                    await ApplyLockoutPolicyAsync(user, ipAddress);
                    await _userManager.UpdateAsync(user);
                }

                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Verificar si el usuario está bloqueado
            if (user.IsLockedOut)
            {
                _logger.LogWarning("Intento de login de usuario bloqueado: {User} desde IP {Ip}", request.Username, ipAddress);
                throw new UnauthorizedAccessException("Usuario bloqueado temporalmente");
            }

            // Verificar si el usuario está activo
            if (!user.IsActive)
            {
                _logger.LogWarning("Login bloqueado para usuario inactivo: {User} desde IP {Ip}", request.Username, ipAddress);
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refresh = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDays"] ?? "7"))
            };

            _context.RefreshTokens.Add(refresh);
            await _context.SaveChangesAsync();

            // Registrar login exitoso
            user.RecordSuccessfulLogin();
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Login exitoso para {User} desde IP {Ip}", request.Username, ipAddress);

            return new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenMinutes"] ?? "15")),
                Roles = roles.ToList(),
                UserId = user.Id
            };
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(string token, string ipAddress)
        {
            _logger.LogInformation("Intento de refresh token desde IP: {Ip}", ipAddress);

            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
            if (existing == null || !existing.IsActive)
            {
                _logger.LogWarning("Refresh token inválido o inactivo desde IP {Ip}", ipAddress);
                return null;
            }

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null) return null;

            // Revoke current and replace
            existing.Revoked = true;
            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.RevokedByIp = ipAddress;
            var newToken = _tokenService.GenerateRefreshToken();
            existing.ReplacedByToken = newToken;

            var newRefresh = new RefreshToken
            {
                Token = newToken,
                UserId = existing.UserId,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDays"] ?? "7"))
            };

            _context.RefreshTokens.Add(newRefresh);
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, roles);

            _logger.LogInformation("Refresh token renovado para userId {UserId} desde IP {Ip}", user.Id, ipAddress);

            return new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = newToken,
                Expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenMinutes"] ?? "15")),
                Roles = roles.ToList(),
                UserId = user.Id
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken, string ipAddress)
        {
            _logger.LogInformation("Logout solicitado desde IP: {Ip}", ipAddress);

            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (existing == null || !existing.IsActive) return false;

            existing.Revoked = true;
            existing.RevokedAtUtc = DateTime.UtcNow;
            existing.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token revocado para userId {UserId} desde IP {Ip}", existing.UserId, ipAddress);
            return true;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Intento de registro para usuario: {Email}", request.Email);

            var user = new User
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("Error en registro de usuario {Email}: {Errors}", request.Email, errors);
                throw new ApplicationException($"Error al crear usuario: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("Usuario registrado exitosamente: {Email}", request.Email);

            return true;
        }

        /// <summary>
        /// Aplica la política de bloqueo automático por intentos fallidos
        /// </summary>
        private async Task ApplyLockoutPolicyAsync(User user, string ipAddress)
        {
            const int maxFailedAttempts = 5;
            const int lockoutMinutes = 15;

            // Verificar si debe bloquearse
            if (user.FailedLoginAttempts >= maxFailedAttempts)
            {
                user.LockOut(lockoutMinutes);

                _logger.LogWarning("Usuario {UserName} bloqueado automáticamente por {FailedAttempts} intentos fallidos desde IP {Ip}",
                    user.UserName, user.FailedLoginAttempts, ipAddress);

                // Auditoría de bloqueo automático
                await _auditService.LogAsync("system", "USER_LOCKED_AUTO", "User", user.Id,
                    $"Usuario bloqueado automáticamente por {user.FailedLoginAttempts} intentos fallidos", ipAddress);
            }
        }
    }
}
