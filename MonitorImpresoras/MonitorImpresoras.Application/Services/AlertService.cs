using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Enums;
using MonitorImpresoras.Domain.Interfaces;

namespace MonitorImpresoras.Application.Services
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly ILogger<AlertService> _logger;

        public AlertService(
            IAlertRepository alertRepository,
            IPrinterRepository printerRepository,
            ILogger<AlertService> logger)
        {
            _alertRepository = alertRepository;
            _printerRepository = printerRepository;
            _logger = logger;
        }

        public async Task<Alert> CreateAlertAsync(CreateAlertDTO alertDto)
        {
            try
            {
                // Verificar si la impresora existe
                var printer = await _printerRepository.GetByIdAsync(alertDto.PrinterId);
                if (printer == null)
                {
                    throw new ArgumentException($"La impresora con ID {alertDto.PrinterId} no existe");
                }

                var alert = new Alert
                {
                    PrinterId = alertDto.PrinterId,
                    Type = alertDto.Type,
                    Description = alertDto.Description ?? GetDefaultDescription(alertDto.Type),
                    Severity = alertDto.Severity,
                    Status = AlertStatus.Pending,
                    DetectedAt = alertDto.DetectedAt,
                    CreatedAt = DateTime.UtcNow
                };

                var createdAlert = await _alertRepository.AddAsync(alert);
                _logger.LogInformation("Alerta creada: {AlertId} para impresora {PrinterId}", createdAlert.Id, alertDto.PrinterId);

                return createdAlert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando alerta para impresora {PrinterId}", alertDto.PrinterId);
                throw;
            }
        }

        public async Task<Alert?> GetAlertByIdAsync(int id)
        {
            return await _alertRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Alert>> GetAlertsByPrinterAsync(int printerId)
        {
            return await _alertRepository.GetByPrinterIdAsync(printerId);
        }

        public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
        {
            return await _alertRepository.GetActiveAlertsAsync();
        }

        public async Task<IEnumerable<Alert>> GetActiveAlertsByPrinterAsync(int printerId)
        {
            return await _alertRepository.GetActiveAlertsByPrinterAsync(printerId);
        }

        public async Task<Alert> UpdateAlertAsync(int id, UpdateAlertDTO alertDto)
        {
            try
            {
                var existingAlert = await _alertRepository.GetByIdAsync(id);
                if (existingAlert == null)
                {
                    throw new ArgumentException($"Alerta con ID {id} no encontrada");
                }

                // Actualizar propiedades si tienen valor
                if (!string.IsNullOrEmpty(alertDto.Description))
                {
                    existingAlert.Description = alertDto.Description;
                }

                if (alertDto.Severity.HasValue)
                {
                    existingAlert.Severity = alertDto.Severity.Value;
                }

                if (alertDto.Status.HasValue)
                {
                    existingAlert.Status = alertDto.Status.Value;
                }

                if (alertDto.ResolvedAt.HasValue)
                {
                    existingAlert.ResolvedAt = alertDto.ResolvedAt;
                }

                existingAlert.UpdatedAt = DateTime.UtcNow;

                var updatedAlert = await _alertRepository.UpdateAsync(existingAlert);
                _logger.LogInformation("Alerta actualizada: {AlertId}", id);

                return updatedAlert;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando alerta {AlertId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAlertAsync(int id)
        {
            try
            {
                var alert = await _alertRepository.GetByIdAsync(id);
                if (alert == null)
                {
                    return false;
                }

                await _alertRepository.DeleteAsync(id);
                _logger.LogInformation("Alerta eliminada: {AlertId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando alerta {AlertId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Alert>> GetAlertsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _alertRepository.GetByDateRangeAsync(startDate, endDate);
        }

        public async Task<AlertStatisticsDTO> GetAlertStatisticsAsync()
        {
            var alerts = await _alertRepository.GetAllAsync();

            var statistics = new AlertStatisticsDTO
            {
                TotalAlerts = alerts.Count(),
                ActiveAlerts = alerts.Count(a => a.Status == AlertStatus.Pending),
                ResolvedAlerts = alerts.Count(a => a.Status == AlertStatus.Resolved),
                AlertsByType = alerts.GroupBy(a => a.Type.ToString())
                                    .ToDictionary(g => g.Key, g => g.Count()),
                AlertsByStatus = alerts.GroupBy(a => a.Status.ToString())
                                     .ToDictionary(g => g.Key, g => g.Count()),
                AlertsBySeverity = alerts.GroupBy(a => a.Severity.ToString())
                                       .ToDictionary(g => g.Key, g => g.Count())
            };

            return statistics;
        }

        private string GetDefaultDescription(AlertType alertType)
        {
            return alertType switch
            {
                AlertType.PrinterOffline => "Impresora fuera de línea",
                AlertType.LowToner => "Nivel de tóner bajo",
                AlertType.PaperJam => "Papel atascado",
                AlertType.OutOfPaper => "Bandeja de papel vacía",
                AlertType.GeneralError => "Error general en la impresora",
                _ => "Alerta de impresora"
            };
        }
    }
}
