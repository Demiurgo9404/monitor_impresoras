using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Models;
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
        Task<IEnumerable<Printer>> GetAllPrintersAsync();

        /// <summary>
        /// Gets a printer by ID
        /// </summary>
        Task<Printer?> GetPrinterByIdAsync(Guid id);

        /// <summary>
        /// Creates a new printer
        /// </summary>
        Task<Printer> AddPrinterAsync(Printer printer);

        /// <summary>
        /// Updates an existing printer
        /// </summary>
        Task<bool> UpdatePrinterAsync(Printer printer);

        /// <summary>
        /// Deletes a printer
        /// </summary>
        Task<bool> DeletePrinterAsync(Guid id);

        /// <summary>
        /// Checks the status of a printer by IP address
        /// </summary>
        Task<bool> CheckPrinterStatusAsync(string ipAddress);

        /// <summary>
        /// Gets the status of a printer with detailed information
        /// </summary>
        Task<PrinterStatusDto> GetPrinterStatusAsync(Guid printerId);

        /// <summary>
        /// Scans the network for new printers
        /// </summary>
        Task<IEnumerable<Printer>> ScanNetworkForPrintersAsync();

        /// <summary>
        /// Gets printer statistics
        /// </summary>
        Task<PrinterStatsDto> GetPrinterStatsAsync();
    }
}
