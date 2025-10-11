using QOPIQ.Application.DTOs;
using QOPIQ.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.Application.Interfaces
{
    /// <summary>
    /// Service interface for printer management
    /// </summary>
    public interface IPrinterService
    {
        /// <summary>
        /// Gets all printers
        /// </summary>
        Task<IEnumerable<PrinterDto>> GetAllAsync();

        /// <summary>
        /// Gets a printer by ID
        /// </summary>
        Task<PrinterDto> GetPrinterByIdAsync(Guid id);

        /// <summary>
        /// Creates a new printer
        /// </summary>
        Task AddPrinterAsync(PrinterCreateDto dto);

        /// <summary>
        /// Updates an existing printer
        /// </summary>
        Task UpdatePrinterAsync(Printer printer);

        /// <summary>
        /// Deletes a printer
        /// </summary>
        Task DeletePrinterAsync(Guid id);

        /// <summary>
        /// Gets the status of a printer with detailed information
        /// </summary>
        Task<PrinterStatusDto> GetPrinterStatusAsync(Guid id);

        /// <summary>
        /// Gets printer statistics
        /// </summary>
        Task<PrinterStatsDto> GetPrinterStatisticsAsync();
    }
}
