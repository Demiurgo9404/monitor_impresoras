using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;
using MonitorImpresoras.Domain.Enums;
using MonitorImpresoras.Domain.DTOs;
using MonitorImpresoras.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Motor de alertas inteligentes para impresoras
    /// </summary>
    public class AlertEngineService : IAlertEngineService
    {
        private readonly IAlertRepository _alertRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly IConsumableRepository _consumableRepository;
        private readonly IAlertService _alertService;
        private readonly ILogger<AlertEngineService> _logger;

        public AlertEngineService(
            IAlertRepository alertRepository,
            IPrinterRepository printerRepository,
            IConsumableRepository consumableRepository,
            IAlertService alertService,
            ILogger<AlertEngineService> logger)
        {
            _alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _consumableRepository = consumableRepository ?? throw new ArgumentNullException(nameof(consumableRepository));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Procesa todas las reglas de alerta activas
        /// </summary>
        public async Task ProcessAlertsAsync()
        {
            var alertsGenerated = 0;
            _logger.LogInformation("Starting alert processing cycle");

            try
            {
                // Verificar impresoras offline
                var offlinePrinters = await _printerRepository.GetOfflinePrintersAsync();
                foreach (var printer in offlinePrinters)
                {
                    if (await CanTriggerAlertAsync(printer.Id, "printer_offline"))
                    {
                        await CreateAlertAsync(
                            $"Impresora {printer.Name} está fuera de línea",
                            $"La impresora {printer.Name} no ha reportado actividad desde {printer.LastSeen}",
                            printer,
                            AlertType.PrinterOffline,
                            AlertSeverity.Medium
                        );
                        alertsGenerated++;
                    }
                }

                // Verificar consumibles bajos
                var lowConsumables = await _consumableRepository.GetLowConsumablesAsync();
                foreach (var consumable in lowConsumables)
                {
                    if (await CanTriggerAlertAsync(consumable.PrinterId, "low_consumable"))
                    {
                        await CreateAlertAsync(
                            $"Consumible bajo: {consumable.PartName}",
                            $"El consumible {consumable.PartName} está al {consumable.CurrentLevel}%",
                            null,
                            AlertType.LowConsumable,
                            AlertSeverity.Low
                        );
                        alertsGenerated++;
                    }
                }

                // Verificar errores de impresoras
                var errorPrinters = await _printerRepository.GetPrintersWithErrorsAsync();
                foreach (var printer in errorPrinters)
                {
                    if (await CanTriggerAlertAsync(printer.Id, "printer_error"))
                    {
                        await CreateAlertAsync(
                            $"Error en impresora: {printer.Name}",
                            $"La impresora {printer.Name} reporta un error en el sistema",
                            printer,
                            AlertType.PrinterError,
                            AlertSeverity.High
                        );
                        alertsGenerated++;
                    }
                }

                _logger.LogInformation("Alert processing completed. Generated {Count} alerts", alertsGenerated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during alert processing");
                throw new ApplicationException("Failed to process alerts", ex);
            }
        }

        /// <summary>
        /// Verifica si se puede generar una alerta (evita spam)
        /// </summary>
        /// <param name="entityId">ID de la entidad relacionada</param>
        /// <param name="alertType">Tipo de alerta</param>
        /// <returns>True si se puede generar la alerta</returns>
        private async Task<bool> CanTriggerAlertAsync(Guid entityId, string alertType)
        {
            try
            {
                // Verificar si ya existe una alerta reciente del mismo tipo para la misma entidad
                var recentAlerts = await _alertRepository.GetRecentAlertsByTypeAndEntityAsync(entityId, alertType, TimeSpan.FromHours(1));

                if (recentAlerts.Any())
                {
                    _logger.LogDebug("Alert not triggered - recent alert exists for entity {EntityId}, type {AlertType}", entityId, alertType);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if alert can be triggered for entity {EntityId}, type {AlertType}", entityId, alertType);
                // En caso de error, permitir la alerta para no perder notificaciones importantes
                return true;
            }
        }

        /// <summary>
        /// Crea una nueva alerta
        /// </summary>
        /// <param name="title">Título de la alerta</param>
        /// <param name="message">Mensaje de la alerta</param>
        /// <param name="printer">Impresora relacionada (opcional)</param>
        /// <param name="type">Tipo de alerta</param>
        /// <param name="severity">Severidad de la alerta</param>
        private async Task CreateAlertAsync(string title, string message, Printer? printer, AlertType type, AlertSeverity severity)
        {
            try
            {
                var createAlertDto = new CreateAlertDTO
                {
                    TenantId = Guid.Empty, // Default tenant
                    Type = type,
                    Title = title,
                    Description = message,
                    Severity = severity,
                    IsAutoGenerated = true,
                    Source = "System"
                };

                await _alertService.CreateAlertAsync(createAlertDto);

                _logger.LogInformation(
                    "Alert created successfully. Title: {Title}, Type: {Type}, Severity: {Severity}, Printer: {PrinterName}",
                    title, type, severity, printer?.Name ?? "None");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert: {Title}", title);
                throw;
            }
        }
    }
}
