using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using System.Net;
using System.Security.Cryptography;
using System.Collections.Generic;
using BCrypt.Net;

namespace QOPIQ.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpirationInMinutes;
        private readonly int _refreshTokenExpirationInDays;

        public AuthService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userRepository = unitOfWork.Users;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            
            _jwtSecret = _configuration["JwtSettings:SecretKey"] 
                ?? throw new ArgumentNullException("JWT Secret Key is not configured");
            _jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "QOPIQ.API";
            _jwtAudience = _configuration["JwtSettings:Audience"] ?? "QOPIQ.Client";
            _jwtExpirationInMinutes = int.TryParse(_configuration["JwtSettings:ExpirationInMinutes"], out var exp) 
                ? exp : 60;
            _refreshTokenExpirationInDays = 7;
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning($"Login failed for email: {loginDto.Email}. User not found or inactive.");
                    return null;
                }

                if (!VerifyPassword(loginDto.Password, user.PasswordHash ?? string.Empty))
                {
                    _logger.LogWarning($"Invalid password for user: {user.Email}");
                    return null;
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken(ipAddress);

                // Save refresh token
                user.RefreshTokens.Add(refreshToken);
                
                // Remove old refresh tokens
                RemoveOldRefreshTokens(user);
                
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"User {user.Email} logged in from IP: {ipAddress}");

                return new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = user.RoleType.ToString(),
                    Token = token,
                    RefreshToken = refreshToken.Token.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for email: {loginDto.Email}");
                throw;
            }
        }

        public async Task<UserDto> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(token);
            if (user == null)
            {
                _logger.LogWarning($"Invalid refresh token attempt from IP: {ipAddress}");
                return null;
            }
            
            var refreshToken = user.RefreshTokens?.FirstOrDefault(x => x.Token.ToString() == token);
            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning($"Inactive refresh token attempt for user {user.Email} from IP: {ipAddress}");
                return null;
            }

            // Revoke current token and generate new one
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;

            // Generate new JWT token
            var jwtToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            user.RefreshTokens ??= new List<RefreshToken>();
            user.RefreshTokens.Add(newRefreshToken);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Token refreshed for user: {user.Email}");

            return new UserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                Name = $"{user.FirstName} {user.LastName}".Trim(),
                Role = user.RoleType.ToString(),
                Token = jwtToken,
                RefreshToken = newRefreshToken.Token.ToString()
            };
        }

        public async Task<bool> LogoutAsync(string token)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(token);
            if (user == null) return false;

            var refreshToken = user.RefreshTokens?.FirstOrDefault(x => x.Token.ToString() == token);
            if (refreshToken == null || !refreshToken.IsActive) return false;

            // Revoke token
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"User {user.Email} logged out");
            
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtIssuer,
                    ValidAudience = _jwtAudience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                return false;
            }
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration attempt with existing email: {registerDto.Email}");
                    throw new InvalidOperationException("User with this email already exists");
                }

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = registerDto.Email,
                    UserName = registerDto.Email,
                    FirstName = registerDto.Name.Split(' ').FirstOrDefault() ?? registerDto.Name,
                    LastName = registerDto.Name.Split(' ').Skip(1).FirstOrDefault() ?? string.Empty,
                    IsActive = true,
                    RoleType = UserRoleType.User, // Default role
                    PasswordHash = HashPassword(registerDto.Password)
                };

                // Save user
                await _userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"New user registered: {user.Email}");

                return new UserDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = user.RoleType.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during registration for email: {registerDto.Email}");
                throw;
            }
        }

        #region Private Helper Methods

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.RoleType.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationInMinutes),
                Issuer = _jwtIssuer,
                Audience = _jwtAudience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationInDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private void RemoveOldRefreshTokens(User user)
        {
            // Remove old expired refresh tokens
            if (user.RefreshTokens != null)
            {
                var tokensToRemove = user.RefreshTokens.Where(x => 
                    !x.IsActive && 
                    x.CreatedAt.AddDays(_refreshTokenExpirationInDays * 2) <= DateTime.UtcNow).ToList();
                
                foreach (var tokenToRemove in tokensToRemove)
                {
                    user.RefreshTokens.Remove(tokenToRemove);
                }
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        #endregion
    }
}
