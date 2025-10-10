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
        /// Establece el ID del tenant actual.
        /// </summary>
        /// <param name="tenantId">The tenant ID to set.</param>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="tenantId"/> is null or whitespace.</exception>
        void SetTenant(string tenantId);
    }
}
