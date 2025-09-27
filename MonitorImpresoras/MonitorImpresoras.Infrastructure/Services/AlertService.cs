using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.DTOs;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly ISignalRNotificationService _signalRService;

        public AlertService(
            IAlertRepository alertRepository,
            IPrinterRepository printerRepository,
            ISignalRNotificationService signalRService)
        {
            _alertRepository = alertRepository;
            _printerRepository = printerRepository;
            _signalRService = signalRService;
        }

        public async Task<AlertDTO> GetAlertByIdAsync(Guid id)
        {
            var alert = await _alertRepository.GetByIdAsync(id);
            return alert != null ? MapToAlertDTO(alert) : null;
        }

        public async Task<IEnumerable<AlertDTO>> GetAlertsByFilterAsync(AlertFilterDTO filter)
        {
            // Implementation for filtering alerts
            var alerts = await _alertRepository.GetAllAsync();
            return alerts.Select(MapToAlertDTO);
        }

        public async Task<AlertDTO> CreateAlertAsync(CreateAlertDTO alertDto)
        {
            var alert = new Alert
            {
                Type = alertDto.Type,
                Title = alertDto.Title,
                Message = alertDto.Message,
                PrinterId = alertDto.PrinterId,
                Source = alertDto.Source,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                Metadata = alertDto.Metadata
            };

            await _alertRepository.AddAsync(alert);

            var alertResult = MapToAlertDTO(alert);

            // Send SignalR notification for new alert
            await _signalRService.NotifyNewAlert(alertResult);

            return alertResult;
        }

        public async Task<AlertDTO> UpdateAlertAsync(Guid id, UpdateAlertDTO alertDto, string userId)
        {
            var alert = await _alertRepository.GetByIdAsync(id);
            if (alert == null)
                return null;

            var previousStatus = alert.Status;

            alert.Status = alertDto.Status ?? alert.Status;
            alert.ResolutionNotes = alertDto.ResolutionNotes ?? alert.ResolutionNotes;
            alert.UpdatedAt = DateTime.UtcNow;

            await _alertRepository.UpdateAsync(alert);

            var alertResult = MapToAlertDTO(alert);

            // Send SignalR notifications based on status changes
            if (alert.Status == "Acknowledged" && previousStatus != "Acknowledged")
            {
                await _signalRService.NotifyAlertAcknowledged(id, userId, "System");
            }
            else if (alert.Status == "Resolved" && previousStatus != "Resolved")
            {
                await _signalRService.NotifyAlertResolved(id, userId, "System", alert.ResolutionNotes);
            }

            return alertResult;
        }

        public async Task<AlertStatsDTO> GetAlertStatsAsync()
        {
            var alerts = await _alertRepository.GetAllAsync();

            return new AlertStatsDTO
            {
                TotalAlerts = alerts.Count(),
                ActiveAlerts = alerts.Count(a => a.Status == "Active"),
                AcknowledgedAlerts = alerts.Count(a => a.Status == "Acknowledged"),
                ResolvedAlerts = alerts.Count(a => a.Status == "Resolved"),
                CriticalAlerts = alerts.Count(a => a.Type.Contains("Critical") || a.Type.Contains("Error")),
                WarningAlerts = alerts.Count(a => a.Type.Contains("Warning")),
                InfoAlerts = alerts.Count(a => a.Type.Contains("Info"))
            };
        }

        public async Task AcknowledgeAlertAsync(Guid id, string userId)
        {
            var alert = await _alertRepository.GetByIdAsync(id);
            if (alert == null || alert.Status == "Resolved")
                return;

            alert.Status = "Acknowledged";
            alert.AcknowledgedBy = userId;
            alert.AcknowledgedAt = DateTime.UtcNow;
            alert.UpdatedAt = DateTime.UtcNow;

            await _alertRepository.UpdateAsync(alert);

            // Send SignalR notification
            await _signalRService.NotifyAlertAcknowledged(id, userId, "System");
        }

        public async Task ResolveAlertAsync(Guid id, string resolutionNotes, string userId)
        {
            var alert = await _alertRepository.GetByIdAsync(id);
            if (alert == null)
                return;

            alert.Status = "Resolved";
            alert.ResolutionNotes = resolutionNotes;
            alert.UpdatedAt = DateTime.UtcNow;

            await _alertRepository.UpdateAsync(alert);

            // Send SignalR notification
            await _signalRService.NotifyAlertResolved(id, userId, "System", resolutionNotes);
        }

        private AlertDTO MapToAlertDTO(Alert alert)
        {
            return new AlertDTO
            {
                Id = alert.Id,
                Type = alert.Type,
                Status = alert.Status,
                Title = alert.Title,
                Message = alert.Message,
                PrinterId = alert.PrinterId,
                PrinterName = alert.Printer?.Name,
                AcknowledgedBy = alert.AcknowledgedBy,
                AcknowledgedAt = alert.AcknowledgedAt,
                ResolutionNotes = alert.ResolutionNotes,
                Source = alert.Source,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt
            };
        }
    }
}
