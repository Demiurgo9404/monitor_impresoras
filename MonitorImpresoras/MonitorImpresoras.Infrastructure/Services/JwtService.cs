using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio JWT
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _secretKey = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? "QopiqAPI";
            _audience = _configuration["Jwt:Audience"] ?? "QopiqClient";
            _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "1440");
        }

        public string GenerateToken(User user, string tenantId, string[] permissions)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Email, user.Email ?? string.Empty),
                    new(ClaimTypes.Name, user.GetFullName()),
                    new(QopiqClaims.TenantId, tenantId),
                    new(QopiqClaims.Role, user.Role),
                    new(QopiqClaims.FullName, user.GetFullName()),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Agregar CompanyId si existe
                if (user.CompanyId.HasValue)
                {
                    claims.Add(new Claim(QopiqClaims.CompanyId, user.CompanyId.Value.ToString()));
                }

                // Agregar permisos
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim(QopiqClaims.Permissions, permission));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogDebug("JWT token generated for user {UserId} in tenant {TenantId}", user.Id, tenantId);
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                if (validatedToken is JwtSecurityToken jwtToken && 
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public ClaimsPrincipal? GetClaimsFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                var claims = jsonToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims);
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading claims from token");
                return null;
            }
        }

        public string? GetTenantIdFromToken(string token)
        {
            var claims = GetClaimsFromToken(token);
            return claims?.FindFirst(QopiqClaims.TenantId)?.Value;
        }

        public string? GetUserIdFromToken(string token)
        {
            var claims = GetClaimsFromToken(token);
            return claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                
                return jsonToken.ValidTo <= DateTime.UtcNow;
            }
            catch
            {
                return true; // Si no se puede leer, consideramos que está expirado
            }
        }
    }
}
