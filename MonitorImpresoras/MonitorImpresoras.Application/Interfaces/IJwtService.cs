using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;
using System.Security.Claims;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para generación y validación de JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Genera un token JWT para un usuario
        /// </summary>
        string GenerateToken(User user, string tenantId, string[] permissions);

        /// <summary>
        /// Genera un refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Valida un token JWT
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Obtiene claims de un token
        /// </summary>
        ClaimsPrincipal? GetClaimsFromToken(string token);

        /// <summary>
        /// Extrae el tenant ID de un token
        /// </summary>
        string? GetTenantIdFromToken(string token);

        /// <summary>
        /// Extrae el user ID de un token
        /// </summary>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// Verifica si un token ha expirado
        /// </summary>
        bool IsTokenExpired(string token);
    }
}
