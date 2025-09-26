using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    /// <summary>
    /// Controlador para dashboard y métricas del sistema
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IPrinterMonitoringService _monitoringService;

        public DashboardController(IPrinterMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        /// <summary>
        /// Obtiene métricas generales del sistema
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetSystemMetrics()
        {
            try
            {
                var stats = await _monitoringService.GetMonitoringStatsAsync();

                var metrics = new
                {
                    Printers = new
                    {
                        Total = stats.TotalPrinters,
                        Online = stats.OnlinePrinters,
                        Offline = stats.OfflinePrinters,
                        OnlinePercentage = stats.TotalPrinters > 0 ? (stats.OnlinePrinters * 100.0 / stats.TotalPrinters) : 0
                    },
                    Activity = new
                    {
                        RecentJobs = stats.RecentJobs,
                        TotalPages24h = stats.TotalPages24h,
                        AveragePagesPerHour = stats.TotalPages24h / 24.0
                    },
                    Alerts = new
                    {
                        Active = 2, // Simulado
                        Resolved = 15, // Simulado
                        Total = 17  // Simulado
                    },
                    LastUpdate = stats.LastUpdate
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene impresoras recientes con actividad
        /// </summary>
        [HttpGet("recent-printers")]
        public async Task<IActionResult> GetRecentPrinters()
        {
            try
            {
                var recentPrinters = new List<object>
                {
                    new
                    {
                        Id = 1,
                        Name = "HP LaserJet 4000",
                        Model = "LaserJet Pro 4000",
                        IpAddress = "192.168.1.100",
                        Status = "Online",
                        LastActivity = DateTime.UtcNow.AddMinutes(-15),
                        PageCount = 15420,
                        TonerLevel = 85,
                        Location = "Oficina Principal"
                    },
                    new
                    {
                        Id = 2,
                        Name = "Epson WF-7720",
                        Model = "WorkForce Pro WF-7720",
                        IpAddress = "192.168.1.101",
                        Status = "Offline",
                        LastActivity = DateTime.UtcNow.AddHours(-2),
                        PageCount = 8934,
                        TonerLevel = 45,
                        Location = "Sala de Juntas"
                    },
                    new
                    {
                        Id = 3,
                        Name = "Brother HL-L3270CDW",
                        Model = "HL-L3270CDW",
                        IpAddress = "192.168.1.102",
                        Status = "Online",
                        LastActivity = DateTime.UtcNow.AddMinutes(-5),
                        PageCount = 5678,
                        TonerLevel = 92,
                        Location = "Departamento de Ventas"
                    }
                };

                return Ok(recentPrinters);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene trabajos de impresión recientes
        /// </summary>
        [HttpGet("recent-jobs")]
        public async Task<IActionResult> GetRecentJobs()
        {
            try
            {
                var recentJobs = new List<object>
                {
                    new
                    {
                        Id = 1,
                        PrinterName = "HP LaserJet 4000",
                        DocumentName = "Reporte_Mensual_2024.pdf",
                        UserName = "María García",
                        Pages = 25,
                        Status = "Completado",
                        StartedAt = DateTime.UtcNow.AddMinutes(-30),
                        CompletedAt = DateTime.UtcNow.AddMinutes(-28),
                        Cost = 1.25m
                    },
                    new
                    {
                        Id = 2,
                        PrinterName = "Epson WF-7720",
                        DocumentName = "Presentacion_Clientes.pptx",
                        UserName = "Carlos Rodríguez",
                        Pages = 15,
                        Status = "Completado",
                        StartedAt = DateTime.UtcNow.AddMinutes(-45),
                        CompletedAt = DateTime.UtcNow.AddMinutes(-43),
                        Cost = 0.75m
                    },
                    new
                    {
                        Id = 3,
                        PrinterName = "Brother HL-L3270CDW",
                        DocumentName = "Factura_001234.pdf",
                        UserName = "Ana López",
                        Pages = 3,
                        Status = "Completado",
                        StartedAt = DateTime.UtcNow.AddMinutes(-10),
                        CompletedAt = DateTime.UtcNow.AddMinutes(-9),
                        Cost = 0.15m
                    }
                };

                return Ok(recentJobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene estadísticas de costos por departamento
        /// </summary>
        [HttpGet("cost-by-department")]
        public async Task<IActionResult> GetCostByDepartment()
        {
            try
            {
                var costStats = new List<object>
                {
                    new { Department = "Ventas", Cost = 125.50m, Jobs = 45, Percentage = 35.2 },
                    new { Department = "Administración", Cost = 89.25m, Jobs = 32, Percentage = 25.1 },
                    new { Department = "Gerencia", Cost = 67.80m, Jobs = 18, Percentage = 19.0 },
                    new { Department = "Recursos Humanos", Cost = 45.30m, Jobs = 12, Percentage = 12.7 },
                    new { Department = "IT", Cost = 28.75m, Jobs = 8, Percentage = 8.0 }
                };

                return Ok(costStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene información del sistema
        /// </summary>
        [HttpGet("system-info")]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                var systemInfo = new
                {
                    Version = "1.0.0",
                    Environment = "Demo",
                    Uptime = TimeSpan.FromHours(24.5),
                    Database = new
                    {
                        Type = "PostgreSQL",
                        Status = "Connected",
                        Size = "2.4 GB"
                    },
                    Features = new List<string>
                    {
                        "Monitoreo SNMP",
                        "Alertas Inteligentes",
                        "Reportes Automáticos",
                        "Multi-tenant",
                        "Dashboard en Tiempo Real"
                    },
                    LastMaintenance = DateTime.UtcNow.AddDays(-2)
                };

                return Ok(systemInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
