using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para gestión avanzada de usuarios
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Obtiene una lista paginada de usuarios con filtros opcionales
        /// </summary>
        Task<PagedResult<UserDto>> GetUsersAsync(
            string? searchTerm = null,
            bool? isActive = null,
            string? role = null,
            int page = 1,
            int pageSize = 10);

        /// <summary>
        /// Obtiene detalles completos de un usuario específico
        /// </summary>
        Task<UserDetailDto?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        Task<bool> AssignRoleToUserAsync(string userId, string roleName, string performedByUserId);

        /// <summary>
        /// Remueve un rol de un usuario
        /// </summary>
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleName, string performedByUserId);

        /// <summary>
        /// Bloquea un usuario
        /// </summary>
        Task<bool> BlockUserAsync(string userId, string performedByUserId, string reason = "Bloqueado por administrador");

        /// <summary>
        /// Desbloquea un usuario
        /// </summary>
        Task<bool> UnblockUserAsync(string userId, string performedByUserId);

        /// <summary>
        /// Resetea la contraseña de un usuario (solo admin)
        /// </summary>
        Task<bool> ResetUserPasswordAsync(string userId, string newPassword, string performedByUserId);

        /// <summary>
        /// Actualiza el perfil de un usuario
        /// </summary>
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto profileDto, string performedByUserId);
    }
}
