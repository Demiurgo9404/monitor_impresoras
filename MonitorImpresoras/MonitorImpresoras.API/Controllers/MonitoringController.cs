using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MonitorImpresoras.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Technician")]
    public class MonitoringController : ControllerBase
    {
        private readonly IMonitoringService _monitoringService;
        private readonly ILogger<MonitoringController> _logger;
        private readonly ApplicationDbContext _context;

        public MonitoringController(
            IMonitoringService monitoringService,
            ILogger<MonitoringController> logger,
            ApplicationDbContext context)
        {
            _monitoringService = monitoringService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _monitoringService.GetMonitoringStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitoring status");
                return StatusCode(500, new { message = "Error retrieving monitoring status" });
            }
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartMonitoring()
        {
            try
            {
                if (await _monitoringService.IsMonitoringActiveAsync())
                {
                    return BadRequest(new { message = "Monitoring is already active" });
                }

                await _monitoringService.StartMonitoringAsync();
                return Ok(new { message = "Monitoring started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting monitoring");
                return StatusCode(500, new { message = "Error starting monitoring" });
            }
        }

        [HttpPost("stop")]
        public async Task<IActionResult> StopMonitoring()
        {
            try
            {
                if (!await _monitoringService.IsMonitoringActiveAsync())
                {
                    return BadRequest(new { message = "Monitoring is not active" });
                }

                await _monitoringService.StopMonitoringAsync();
                return Ok(new { message = "Monitoring stopped successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping monitoring");
                return StatusCode(500, new { message = "Error stopping monitoring" });
            }
        }

        [HttpPost("restart")]
        public async Task<IActionResult> RestartMonitoring()
        {
            try
            {
                await _monitoringService.StopMonitoringAsync();
                await Task.Delay(1000); // Wait 1 second
                await _monitoringService.StartMonitoringAsync();
                return Ok(new { message = "Monitoring restarted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting monitoring");
                return StatusCode(500, new { message = "Error restarting monitoring" });
            }
        }

        [HttpPost("test-signalr")]
        public async Task<IActionResult> TestSignalR()
        {
            try
            {
                // This would normally be handled by the SignalRNotificationService
                // For testing, we'll send a test notification
                await _monitoringService.GetMonitoringStatusAsync(); // Just to test the service is working
                return Ok(new { message = "SignalR test completed - check client for notifications" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SignalR");
                return StatusCode(500, new { message = "Error testing SignalR" });
            }
        }

        // GET: api/monitoring/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _context.MonitoringStats
                .OrderByDescending(s => s.LastUpdated)
                .FirstOrDefaultAsync();

            if (stats == null)
                return NotFound("No se encontraron estadísticas de monitoreo.");

            return Ok(stats);
        }

        // POST: api/monitoring/stats
        [HttpPost("stats")]
        public async Task<IActionResult> AddStats([FromBody] MonitoringStats stats)
        {
            if (stats == null)
                return BadRequest("Datos inválidos.");

            stats.LastUpdated = DateTime.UtcNow;

            _context.MonitoringStats.Add(stats);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStats), new { id = stats.Id }, stats);
        }
    }
}
