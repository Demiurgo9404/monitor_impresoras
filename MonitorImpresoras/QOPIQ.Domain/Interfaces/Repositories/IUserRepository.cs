using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interface for user repository operations
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Obtiene un usuario por su nombre de usuario de manera asíncrona.
        /// </summary>
        /// <param name="userName">Nombre de usuario.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>El usuario encontrado o null si no existe.</returns>
        Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su dirección de correo electrónico.
        /// </summary>
        /// <param name="email">Dirección de correo electrónico del usuario.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>El usuario encontrado o null si no existe.</returns>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a user with the given email exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if user exists, false otherwise</returns>
        Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a user by their refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User if found, null otherwise</returns>
        Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los roles de un usuario por su ID.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Lista de nombres de roles del usuario.</returns>
        Task<IList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario está en un rol específico.
        /// </summary>
        /// <param name="user">Usuario a verificar.</param>
        /// <param name="roleName">Nombre del rol.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>True si el usuario está en el rol, false en caso contrario.</returns>
        Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken = default);
    }
}
