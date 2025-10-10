#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace QOPIQ.Application.Interfaces.MultiTenancy
{
    /// <summary>
    /// Proporciona acceso al tenant actual en el contexto de ejecución.
    /// </summary>
    public interface ITenantAccessor
    {
        /// <summary>
        /// Obtiene un valor que indica si se ha establecido un tenant.
        /// </summary>
        [MemberNotNullWhen(true, nameof(TenantId))]
        bool HasTenant { get; }

        /// <summary>
        /// Obtiene el ID del tenant actual, o null si no se ha establecido ningún tenant.
        /// </summary>
        string? TenantId { get; }

        /// <summary>
        /// Establece el ID del tenant actual y su información adicional.
        /// </summary>
        /// <param name="tenantId">El ID del tenant a establecer.</param>
        /// <param name="tenantInfo">Información adicional del tenant (opcional).</param>
        /// <exception cref="System.ArgumentException">Se lanza cuando <paramref name="tenantId"/> es nulo o vacío.</exception>
        void SetTenant(string tenantId, object? tenantInfo = null);

        /// <summary>
        /// Obtiene el ID del tenant de forma asíncrona.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <returns>El ID del tenant o null si no se puede determinar.</returns>
        Task<string?> GetTenantIdAsync(CancellationToken cancellationToken = default);
    }
}
