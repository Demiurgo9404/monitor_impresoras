using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.Repositories
{
    public interface IPrinterRepository
    {
        /// <summary>
        /// Obtiene una impresora por su ID
        /// </summary>
        Task<Printer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las impresoras
        /// </summary>
        Task<IEnumerable<Printer>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva impresora
        /// </summary>
        Task AddAsync(Printer printer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una impresora existente
        /// </summary>
        Task UpdateAsync(Printer printer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una impresora por su ID
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe una impresora con el ID especificado
        /// </summary>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

