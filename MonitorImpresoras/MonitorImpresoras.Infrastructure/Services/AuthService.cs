using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación multi-tenant
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IJwtService jwtService,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string tenantId)
        {
            try
            {
                // Validar que el tenant existe y está activo
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantKey == tenantId && t.IsActive);

                if (tenant == null)
                {
                    _logger.LogWarning("Login attempt with invalid tenant: {TenantId}", tenantId);
                    throw new UnauthorizedAccessException("Invalid tenant");
                }

                // Buscar usuario en el tenant
                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.TenantId == tenantId && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with invalid email: {Email} in tenant: {TenantId}", loginDto.Email, tenantId);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Verificar contraseña
                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", loginDto.Password);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Login attempt with invalid password for user: {UserId}", user.Id);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Generar permisos basados en el rol
                var permissions = QopiqPermissions.RolePermissions.GetValueOrDefault(user.Role, Array.Empty<string>());

                // Generar tokens
                var token = _jwtService.GenerateToken(user, tenantId, permissions);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Actualizar refresh token en la base de datos
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 días
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successful login for user {UserId} in tenant {TenantId}", user.Id, tenantId);

                return new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(1440), // 24 horas
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.GetFullName(),
                        Role = user.Role,
                        Permissions = permissions,
                        CompanyId = user.CompanyId,
                        CompanyName = user.Company?.Name
                    },
                    Tenant = new TenantInfoDto
                    {
                        TenantId = tenant.TenantKey,
                        Name = tenant.Name,
                        CompanyName = tenant.CompanyName,
                        IsActive = tenant.IsActive,
                        ExpiresAt = tenant.ExpiresAt,
                        Tier = tenant.Tier.ToString(),
                        MaxPrinters = tenant.MaxPrinters,
                        MaxUsers = tenant.MaxUsers
                    }
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email} in tenant: {TenantId}", loginDto.Email, tenantId);
                throw new Exception("Login failed");
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string tenantId)
        {
            try
            {
                // Validar que el tenant existe y está activo
                var tenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.TenantKey == tenantId && t.IsActive);

                if (tenant == null)
                {
                    throw new ArgumentException("Invalid tenant");
                }

                // Verificar que el email no existe en el tenant
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email && u.TenantId == tenantId);

                if (existingUser != null)
                {
                    throw new ArgumentException("User already exists");
                }

                // Verificar límites del tenant
                var userCount = await _context.Users.CountAsync(u => u.TenantId == tenantId);
                if (userCount >= tenant.MaxUsers)
                {
                    throw new ArgumentException("Maximum users limit reached for this tenant");
                }

                // Crear nuevo usuario
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    Email = registerDto.Email,
                    UserName = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Department = registerDto.Department ?? "",
                    Role = QopiqRoles.All.Contains(registerDto.Role) ? registerDto.Role : QopiqRoles.Viewer,
                    CompanyId = registerDto.CompanyId,
                    IsActive = true,
                    EmailConfirmed = true, // Por simplicidad, confirmamos automáticamente
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Hash de la contraseña
                user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {UserId} in tenant: {TenantId}", user.Id, tenantId);

                // Hacer login automático
                var loginDto = new LoginDto
                {
                    Email = registerDto.Email,
                    Password = registerDto.Password
                };

                return await LoginAsync(loginDto, tenantId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email} in tenant: {TenantId}", registerDto.Email, tenantId);
                throw new Exception("Registration failed");
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshDto, string tenantId)
        {
            try
            {
                var userId = _jwtService.GetUserIdFromToken(refreshDto.Token);
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("Invalid token");
                }

                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.IsActive);

                if (user == null || user.RefreshToken != refreshDto.RefreshToken || !user.IsRefreshTokenValid())
                {
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }

                // Generar nuevos tokens
                var permissions = QopiqPermissions.RolePermissions.GetValueOrDefault(user.Role, Array.Empty<string>());
                var newToken = _jwtService.GenerateToken(user, tenantId, permissions);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Actualizar refresh token
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                var tenant = await _context.Tenants.FirstAsync(t => t.TenantKey == tenantId);

                return new AuthResponseDto
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(1440),
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.GetFullName(),
                        Role = user.Role,
                        Permissions = permissions,
                        CompanyId = user.CompanyId,
                        CompanyName = user.Company?.Name
                    },
                    Tenant = new TenantInfoDto
                    {
                        TenantId = tenant.TenantKey,
                        Name = tenant.Name,
                        CompanyName = tenant.CompanyName,
                        IsActive = tenant.IsActive,
                        ExpiresAt = tenant.ExpiresAt,
                        Tier = tenant.Tier.ToString(),
                        MaxPrinters = tenant.MaxPrinters,
                        MaxUsers = tenant.MaxUsers
                    }
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh for tenant: {TenantId}", tenantId);
                throw new Exception("Token refresh failed");
            }
        }

        public async Task<bool> LogoutAsync(string userId, string tenantId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

                if (user != null)
                {
                    user.RefreshToken = "";
                    user.RefreshTokenExpiryTime = null;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User logged out successfully: {UserId}", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto, string tenantId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.IsActive);

                if (user == null)
                {
                    return false;
                }

                // Verificar contraseña actual
                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", changePasswordDto.CurrentPassword);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    throw new UnauthorizedAccessException("Current password is incorrect");
                }

                // Actualizar contraseña
                user.PasswordHash = _passwordHasher.HashPassword(user, changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                // Invalidar refresh tokens existentes
                user.RefreshToken = "";
                user.RefreshTokenExpiryTime = null;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token, string tenantId)
        {
            try
            {
                var claims = _jwtService.ValidateToken(token);
                if (claims == null)
                {
                    return false;
                }

                var tokenTenantId = claims.FindFirst(QopiqClaims.TenantId)?.Value;
                return tokenTenantId == tenantId;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserInfoDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var claims = _jwtService.GetClaimsFromToken(token);
                if (claims == null)
                {
                    return null;
                }

                var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tenantId = claims.FindFirst(QopiqClaims.TenantId)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
                {
                    return null;
                }

                var user = await _context.Users
                    .Include(u => u.Company)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.IsActive);

                if (user == null)
                {
                    return null;
                }

                var permissions = QopiqPermissions.RolePermissions.GetValueOrDefault(user.Role, Array.Empty<string>());

                return new UserInfoDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.GetFullName(),
                    Role = user.Role,
                    Permissions = permissions,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user from token");
                return null;
            }
        }
    }
}
