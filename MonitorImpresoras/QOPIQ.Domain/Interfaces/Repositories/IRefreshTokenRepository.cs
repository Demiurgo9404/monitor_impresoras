using System;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interface for refresh token repository operations
    /// </summary>
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        /// <summary>
        /// Gets a refresh token by its value
        /// </summary>
        /// <param name="token">Token value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Refresh token if found, null otherwise</returns>
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Invalidates all refresh tokens for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the operation</returns>
        Task InvalidateUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if a refresh token is valid
        /// </summary>
        /// <param name="token">Token to check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if token is valid, false otherwise</returns>
        Task<bool> IsValidTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}
