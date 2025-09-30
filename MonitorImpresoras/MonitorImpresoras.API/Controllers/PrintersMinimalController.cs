using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PrintersMinimalController : ControllerBase
    {
        /// <summary>
        /// Obtiene todas las impresoras (baseline: datos de ejemplo)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrinterDto>>> GetAll(CancellationToken ct)
        {
            var demo = new List<PrinterDto>
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
            } as IEnumerable<PrinterDto>;

            await Task.CompletedTask;
            return Ok(demo);
        }
    }
}
