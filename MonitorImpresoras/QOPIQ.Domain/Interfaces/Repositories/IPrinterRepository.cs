using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Interfaces;

namespace QOPIQ.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de impresoras que define las operaciones CRUD y específicas para impresoras.
    /// </summary>
    public interface IPrinterRepository : IRepository<Printer>
    {
        /// <summary>
        /// Obtiene una impresora por su dirección IP de manera asíncrona.
        /// </summary>
        /// <param name="ipAddress">Dirección IP de la impresora.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>La impresora encontrada o null si no existe.</returns>
        Task<Printer> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las impresoras de un departamento específico.
        /// </summary>
        /// <param name="departmentId">ID del departamento.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Lista de impresoras del departamento.</returns>
        Task<IEnumerable<Printer>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las impresoras que necesitan mantenimiento según su estado.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Lista de impresoras que necesitan mantenimiento.</returns>
        Task<IEnumerable<Printer>> GetPrintersNeedingMaintenanceAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las impresoras con bajo nivel de tóner.
        /// </summary>
        /// <param name="threshold">Umbral de tóner bajo (porcentaje).</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Lista de impresoras con tóner bajo.</returns>
        Task<IEnumerable<Printer>> GetPrintersWithLowTonerAsync(int threshold = 20, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca impresoras por nombre o modelo.
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Lista de impresoras que coinciden con el término de búsqueda.</returns>
        Task<IEnumerable<Printer>> SearchPrintersAsync(string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si ya existe una impresora con la misma dirección IP.
        /// </summary>
        /// <param name="ipAddress">Dirección IP a verificar.</param>
        /// <param name="excludeId">ID de la impresora a excluir de la búsqueda (para actualizaciones).</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>True si ya existe una impresora con esa IP, false en caso contrario.</returns>
        Task<bool> ExistsWithIpAddressAsync(string ipAddress, Guid? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una entidad de forma asíncrona.
        /// </summary>
        /// <param name="entity">Entidad a actualizar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>La entidad actualizada.</returns>
        Task<Printer> UpdateAsync(Printer entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una entidad de forma asíncrona.
        /// </summary>
        /// <param name="entity">Entidad a eliminar.</param>
        /// <param name="cancellationToken">Token de cancelación opcional.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task RemoveAsync(Printer entity, CancellationToken cancellationToken = default);
    }
}
