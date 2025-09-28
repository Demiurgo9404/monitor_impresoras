using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using Serilog;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public AuthService(UserManager<User> userManager, RoleManager<Role> roleManager, ITokenService tokenService, IAuditService auditService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _auditService = auditService;
            _context = context;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                // Auditoría de intento fallido
                await _auditService.LogAsync("system", "LOGIN_FAILED", "User", request.Username,
                    $"Intento de login fallido para usuario: {request.Username}", ipAddress, userAgent);
                await RegisterLoginAttempt(user, request.Username, false, ipAddress, userAgent, "Credenciales inválidas");
                throw new UnauthorizedAccessException("Usuario o contraseña incorrectos");
            }

            if (!user.IsActive)
            {
                await _auditService.LogAsync(user.Id, "LOGIN_BLOCKED", "User", user.Id,
                    "Intento de login de usuario inactivo", ipAddress, userAgent);
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            await RegisterLoginAttempt(user, user.UserName!, true, ipAddress, userAgent);

            // Auditoría de login exitoso
            await _auditService.LogAsync(user.Id, "LOGIN_SUCCESS", "User", user.Id,
                $"Login exitoso para usuario: {user.UserName}", ipAddress, userAgent);

            Log.Information("Usuario {UserName} inició sesión desde IP {IpAddress}", user.UserName, ipAddress);

            return new LoginResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Roles = roles.ToList()
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("El correo ya está registrado");

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
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new Role { Name = "User" });

            await _userManager.AddToRoleAsync(user, "User");

            // Auditoría de registro
            await _auditService.LogAsync(user.Id, "USER_REGISTERED", "User", user.Id,
                $"Nuevo usuario registrado: {user.UserName} ({user.Email})");

            Log.Information("Nuevo usuario registrado: {UserName} ({Email})", user.UserName, user.Email);

            return true;
        }

        public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            // Auditoría de refresh token
            await _auditService.LogAsync(user.Id, "REFRESH_TOKEN", "User", user.Id,
                "Token de acceso renovado", ipAddress);

            Log.Information("Token renovado para usuario {UserName}", user.UserName);

            return new LoginResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(30),
                Roles = roles.ToList()
            };
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);

            // Auditoría de logout
            await _auditService.LogAsync(userId, "LOGOUT", "User", userId,
                $"Usuario {user.UserName} cerró sesión");

            Log.Information("Usuario {UserName} cerró sesión", user.UserName);

            return true;
        }

        private async Task RegisterLoginAttempt(User? user, string username, bool success, string ipAddress, string userAgent, string? failureReason = null)
        {
            _context.LoginAttempts.Add(new LoginAttempt
            {
                UserId = user?.Id,
                Username = username,
                Success = success,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                FailureReason = failureReason,
                AttemptDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
