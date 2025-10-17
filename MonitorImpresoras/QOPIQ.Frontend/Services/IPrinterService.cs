using QOPIQ.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.Frontend.Services;

public interface IPrinterService
{
    Task<List<PrinterDto>> GetAllAsync();
    Task<PrinterDto?> GetByIdAsync(Guid id);
    Task<PrinterDto> CreateAsync(PrinterCreateDto printer);
    Task UpdateAsync(PrinterUpdateDto printer);
    Task DeleteAsync(Guid id);
    Task<PrinterStatusDto?> GetPrinterStatusAsync(Guid printerId);
    Task<IEnumerable<PrinterStatusDto>> GetAllStatusesAsync();
    
    // Métodos para el dashboard
    Task<PrinterStatsDto> GetStatsAsync();
    
    // Método para búsqueda
    Task<List<PrinterDto>> SearchAsync(string searchTerm);
}
