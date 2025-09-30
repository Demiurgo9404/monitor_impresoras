using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Contrato mínimo para consultar impresoras en baseline.
    /// </summary>
    public interface IPrinterQueryService
    {
        Task<IEnumerable<PrinterDto>> GetAllAsync(CancellationToken ct = default);
    }
}
