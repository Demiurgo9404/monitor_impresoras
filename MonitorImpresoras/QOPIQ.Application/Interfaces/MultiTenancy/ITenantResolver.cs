using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Application.Interfaces.MultiTenancy
{
    /// <summary>
    /// Define el contrato para resolver el identificador del tenant actual.
    /// </summary>
    public interface ITenantResolver
    {
        /// <summary>
        /// Resuelve el identificador del tenant basado en el contexto actual.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El identificador del tenant o null si no se puede determinar.</returns>
        Task<string?> ResolveTenantIdentifierAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Obtiene el ID del tenant actual.
        /// </summary>
        /// <returns>El ID del tenant actual o null si no se puede determinar.</returns>
        string? GetCurrentTenantId();
        
        /// <summary>
        /// Verifica si un tenant es válido.
        /// </summary>
        /// <param name="tenantId">El ID del tenant a validar.</param>
        /// <returns>True si el tenant es válido, de lo contrario false.</returns>
        Task<bool> IsValidTenantAsync(string tenantId);
        
        /// <summary>
        /// Obtiene la información del tenant actual.
        /// </summary>
        /// <returns>Un objeto que representa la información del tenant actual.</returns>
        Task<object?> GetCurrentTenantAsync();
    }
}
