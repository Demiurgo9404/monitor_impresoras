using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación baseline (sin dependencias externas) para consultar impresoras.
    /// </summary>
    public sealed class PrinterQueryService : IPrinterQueryService
    {
        private static readonly List<PrinterDto> _demo = new()
        {
            new PrinterDto
            {
                Id = 1,
                Name = "HP-BackOffice",
                Description = "Impresora HP BackOffice",
                IpAddress = "192.168.1.50",
                SerialNumber = "HP-BO-001",
                Model = "HP LaserJet",
                Brand = "HP",
                Location = "BackOffice",
                Department = "Ops",
                Status = "Online"
            },
            new PrinterDto
            {
                Id = 2,
                Name = "Brother-Front",
                Description = "Impresora Brother FrontDesk",
                IpAddress = "192.168.1.51",
                SerialNumber = "BR-FR-002",
                Model = "Brother HL",
                Brand = "Brother",
                Location = "Front Desk",
                Department = "Admin",
                Status = "Online"
            }
        };

        public Task<IEnumerable<PrinterDto>> GetAllAsync(CancellationToken ct = default)
            => Task.FromResult<IEnumerable<PrinterDto>>(_demo);

        public Task<PrinterDto?> GetByIdAsync(int id, CancellationToken ct = default)
            => Task.FromResult(_demo.Find(p => p.Id == id));
    }
}
