using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para gestión de permisos granulares (claims) de usuarios
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Obtiene todos los claims activos de un usuario
        /// </summary>
        Task<IEnumerable<UserClaimDto>> GetUserClaimsAsync(string userId);

        /// <summary>
        /// Asigna un claim específico a un usuario
        /// </summary>
        Task<bool> AssignClaimToUserAsync(
            string userId,
            string claimType,
            string claimValue,
            string? description = null,
            string? category = null,
            DateTime? expiresAtUtc = null,
            string performedByUserId = "system");

        /// <summary>
        /// Revoca un claim específico de un usuario
        /// </summary>
        Task<bool> RevokeClaimFromUserAsync(string userId, string claimType, string performedByUserId = "system");

        /// <summary>
        /// Revoca todos los claims de un usuario
        /// </summary>
        Task<bool> RevokeAllClaimsFromUserAsync(string userId, string performedByUserId = "system", string reason = "Revocación masiva");

        /// <summary>
        /// Obtiene la lista de claims disponibles en el sistema
        /// </summary>
        Task<IEnumerable<ClaimDefinitionDto>> GetAvailableClaimsAsync();

        /// <summary>
        /// Verifica si un usuario tiene un claim específico
        /// </summary>
        Task<bool> UserHasClaimAsync(string userId, string claimType, string claimValue = "true");

        /// <summary>
        /// Obtiene los tipos de claims de un usuario
        /// </summary>
        Task<IEnumerable<string>> GetUserClaimTypesAsync(string userId);

        /// <summary>
        /// Obtiene un diccionario de claims del usuario (tipo -> valor)
        /// </summary>
        Task<Dictionary<string, string>> GetUserClaimsDictionaryAsync(string userId);
    }
}
