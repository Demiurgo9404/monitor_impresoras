using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Contrato mínimo para consultar impresoras en baseline.
    /// </summary>
    public interface IPrinterQueryService
    {
        Task<IEnumerable<PrinterQueryDto>> GetAllAsync(CancellationToken ct = default);
        Task<PrinterQueryDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IEnumerable<PrinterQueryDto> Items, int Total)> SearchAsync(
            string? q,
            int page = 1,
            int pageSize = 20,
            string? orderBy = "Name",
            bool desc = false,
            CancellationToken ct = default);
    }
}
