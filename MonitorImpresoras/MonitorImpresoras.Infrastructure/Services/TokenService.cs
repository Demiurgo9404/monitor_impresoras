using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, IPermissionService permissionService, ILogger<TokenService> logger)
        {
            _config = config;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<string> GenerateAccessTokenAsync(User user, IList<string> roles)
        {
            try
            {
                _logger.LogDebug("Generando token de acceso para usuario: {UserId}", user.Id);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("IsActive", user.IsActive.ToString()),
                    new Claim("FullName", user.GetFullName()),
                    new Claim("Department", user.Department ?? ""),
                    new Claim("ManagePrinters", (roles.Contains("Admin") || user.Department == "IT").ToString()),
                    new Claim("ReadPrinters", "true"), // Todos los usuarios autenticados pueden leer impresoras
                    new Claim("CanDeletePrinters", roles.Contains("Admin").ToString()),
                    new Claim("IssuedAt", DateTime.UtcNow.ToString("o"))
                };

                // Agregar roles como claims
                authClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                // Agregar permisos basados en roles
                if (roles.Contains("Admin"))
                {
                    authClaims.Add(new Claim("Permissions", "FullAccess"));
                    authClaims.Add(new Claim("CanManageUsers", "true"));
                    authClaims.Add(new Claim("CanViewAuditLogs", "true"));
                }
                else if (roles.Contains("User"))
                {
                    authClaims.Add(new Claim("Permissions", "ReadOnly"));
                    authClaims.Add(new Claim("CanManageUsers", "false"));
                    authClaims.Add(new Claim("CanViewAuditLogs", "false"));
                }

                // Agregar claims dinámicos desde la base de datos
                var userClaims = await _permissionService.GetUserClaimsDictionaryAsync(user.Id);
                foreach (var claim in userClaims)
                {
                    authClaims.Add(new Claim(claim.Key, claim.Value));
                }

                _logger.LogDebug("Token generado con {ClaimCount} claims para usuario: {UserId}", authClaims.Count, user.Id);

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:AccessTokenMinutes"] ?? "15")),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar token de acceso para usuario: {UserId}", user.Id);
                throw;
            }
        }

        public string GenerateAccessToken(User user, IList<string> roles)
        {
            // Método legacy para compatibilidad - usar la versión async
            var task = GenerateAccessTokenAsync(user, roles);
            return task.GetAwaiter().GetResult();
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // allow expired
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token inválido");

            return principal;
        }
    }
}
