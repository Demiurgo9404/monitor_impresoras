using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.API.Hubs;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.API.Controllers
{
    public class AlertsController : BaseApiController
    {
        private readonly IAlertService _alertService;
        private readonly IHubContext<PrinterHub> _hubContext;
        private readonly ILogger<AlertsController> _logger;
        private readonly IMapper _mapper;

        public AlertsController(
            IAlertService alertService,
            IHubContext<PrinterHub> hubContext,
            ILogger<AlertsController> logger,
            IMapper mapper)
        {
            _alertService = alertService;
            _hubContext = hubContext;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<AlertDTO>>> GetAlerts(
            [FromQuery] string type = null,
            [FromQuery] string status = null,
            [FromQuery] int? printerId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? isAcknowledged = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new AlertFilterDTO
            {
                Type = type,
                Status = status,
                PrinterId = printerId,
                StartDate = startDate,
                EndDate = endDate,
                IsAcknowledged = isAcknowledged,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var alerts = await _alertService.GetAlertsByFilterAsync(filter);
            return HandleResult(alerts);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<AlertDTO>> GetAlert(int id)
        {
            var alert = await _alertService.GetAlertByIdAsync(id);
            return HandleResult(alert);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<AlertDTO>> CreateAlert(CreateAlertDTO alertDto)
        {
            // Only allow certain alert types to be created manually
            if (string.Equals(alertDto.Type, AlertType.LowConsumable.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(alertDto.Type, AlertType.EmptyConsumable.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("This type of alert cannot be created manually");
            }

            var result = await _alertService.CreateAlertAsync(alertDto);
            
            // Notify connected clients about the new alert
            if (result != null)
            {
                await _hubContext.Clients.Group("technicians")
                    .SendAsync("ReceiveAlert", result);
                    
                if (result.PrinterId.HasValue)
                {
                    await _hubContext.Clients.Group($"printer-{result.PrinterId}")
                        .SendAsync("PrinterAlert", result);
                }
            }

            return CreatedAtAction(nameof(GetAlert), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<AlertDTO>> UpdateAlert(int id, UpdateAlertDTO alertDto)
        {
            var result = await _alertService.UpdateAlertAsync(id, alertDto, UserId);
            
            // Notify connected clients about the updated alert
            if (result != null)
            {
                await _hubContext.Clients.Group("technicians")
                    .SendAsync("AlertUpdated", result);
                    
                if (result.PrinterId.HasValue)
                {
                    await _hubContext.Clients.Group($"printer-{result.PrinterId}")
                        .SendAsync("PrinterAlertUpdated", result);
                }
            }
            
            return HandleResult(result);
        }

        [HttpPost("{id}/acknowledge")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> AcknowledgeAlert(int id)
        {
            await _alertService.AcknowledgeAlertAsync(id, UserId);
            
            // Notify clients that the alert was acknowledged
            await _hubContext.Clients.Group("technicians")
                .SendAsync("AlertAcknowledged", id, UserId, UserEmail);
                
            return NoContent();
        }

        [HttpPost("{id}/resolve")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ResolveAlert(int id, [FromBody] string resolutionNotes)
        {
            await _alertService.ResolveAlertAsync(id, resolutionNotes, UserId);
            
            // Notify clients that the alert was resolved
            await _hubContext.Clients.Group("technicians")
                .SendAsync("AlertResolved", id, UserId, UserEmail);
                
            return NoContent();
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<AlertStatsDTO>> GetAlertStats()
        {
            var stats = await _alertService.GetAlertStatsAsync();
            return HandleResult(stats);
        }

        [HttpGet("printer/{printerId}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<AlertDTO>>> GetPrinterAlerts(int printerId)
        {
            var filter = new AlertFilterDTO
            {
                PrinterId = printerId,
                PageSize = 50 // Limit to most recent 50 alerts
            };

            var alerts = await _alertService.GetAlertsByFilterAsync(filter);
            return HandleResult(alerts);
        }
    }
}
