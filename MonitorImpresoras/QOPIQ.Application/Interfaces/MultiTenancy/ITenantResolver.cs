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
    }
}
