using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QOPIQ.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"] ?? "super-secret-key-for-development-only-change-in-production"));

                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id?.ToString() ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Name, user.Name ?? user.UserName ?? ""),
                    new Claim("tenant_id", user.CompanyId?.ToString() ?? "")
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"] ?? "QOPIQ",
                    audience: _configuration["Jwt:Audience"] ?? "QOPIQ",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                
                _logger.LogInformation("Generated access token for user {UserId}", user.Id);
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user {UserId}", user.Id);
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

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"] ?? "super-secret-key-for-development-only-change-in-production"));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting principal from expired token");
                return null;
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                return principal != null;
            }
            catch
            {
                return false;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<string, string> GetClaimsFromToken(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                var claims = new Dictionary<string, string>();

                if (principal != null)
                {
                    foreach (var claim in principal.Claims)
                    {
                        claims[claim.Type] = claim.Value;
                    }
                }

                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting claims from token");
                return new Dictionary<string, string>();
            }
        }
    }
}

