using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateAccessToken(User user, IList<string> roles)
        {
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

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"] ?? "60")),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // ⚠️ Aquí permitimos tokens expirados
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token inválido");

            return principal;
        }
    }
}
