using System.Threading.Tasks;
using QOPIQ.Domain.Models;

namespace QOPIQ.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Obtiene un token de actualización por su valor
        /// </summary>
        Task<RefreshToken> GetByTokenAsync(string token);

        /// <summary>
        /// Agrega un nuevo token de actualización
        /// </summary>
        Task AddAsync(RefreshToken refreshToken);

        /// <summary>
        /// Elimina un token de actualización
        /// </summary>
        void Remove(RefreshToken refreshToken);

        /// <summary>
        /// Guarda los cambios en el contexto
        /// </summary>
        Task<bool> SaveChangesAsync();
    }
}
