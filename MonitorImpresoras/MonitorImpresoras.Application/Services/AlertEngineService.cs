using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Enums;
using MonitorImpresoras.Domain.Interfaces;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MonitorImpresoras.Application.Services
{
    public class AlertEngineService : IAlertEngineService
    {
        private readonly Timer _timer;
        private readonly IPrinterStatusService _printerStatusService;
        private readonly IAlertService _alertService;
        private readonly ILogger<AlertEngineService> _logger;
        private readonly AlertEngineConfig _config;

        public AlertEngineService(
            IPrinterStatusService printerStatusService,
            IAlertService alertService,
            IOptions<AlertEngineConfig> config,
            ILogger<AlertEngineService> logger)
        {
            _printerStatusService = printerStatusService;
            _alertService = alertService;
            _logger = logger;
            _config = config.Value;

            _timer = new Timer(_config.IntervalMinutes * 60 * 1000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
        }

        public void Start()
        {
            _timer.Start();
            _logger.LogInformation("AlertEngineService iniciado. Intervalo: {Interval} minutos", _config.IntervalMinutes);
        }

        public void Stop()
        {
            _timer.Stop();
            _logger.LogInformation("AlertEngineService detenido");
        }

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            try
            {
                _logger.LogInformation("Ejecutando verificación de estado de impresoras...");
                var printers = await _printerStatusService.GetAllPrintersStatusAsync();

                foreach (var printer in printers)
                {
                    await EvaluatePrinterStatus(printer);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la verificación de estado de impresoras");
            }
        }

        private async Task EvaluatePrinterStatus(PrinterStatusInfo printerStatus)
        {
            try
            {
                // Verificar si la impresora está offline
                if (printerStatus.IsOffline)
                {
                    await CreateAlertIfNotExists(printerStatus, AlertType.PrinterOffline, 
                        "Impresora offline por más de 5 minutos", AlertSeverity.High);
                    return;
                }

                // Verificar bajo nivel de tóner
                if (printerStatus.TonerLevel <= _config.LowTonerThreshold && printerStatus.TonerLevel > 0)
                {
                    await CreateAlertIfNotExists(printerStatus, AlertType.LowToner,
                        $"Nivel de tóner bajo: {printerStatus.TonerLevel}%", AlertSeverity.Medium);
                }

                // Verificar papel atascado
                if (printerStatus.HasPaperJam)
                {
                    await CreateAlertIfNotExists(printerStatus, AlertType.PaperJam,
                        "Papel atascado en la impresora", AlertSeverity.High);
                }

                // Verificar bandeja de papel vacía
                if (printerStatus.IsOutOfPaper)
                {
                    await CreateAlertIfNotExists(printerStatus, AlertType.OutOfPaper,
                        "Bandeja de papel vacía", AlertSeverity.High);
                }

                // Verificar errores generales
                if (printerStatus.HasError)
                {
                    await CreateAlertIfNotExists(printerStatus, AlertType.GeneralError,
                        "Error general en la impresora", AlertSeverity.High);
                }

                // Si la impresora está OK, resolver alertas existentes
                if (!printerStatus.IsOffline && !printerStatus.HasPaperJam && 
                    !printerStatus.IsOutOfPaper && !printerStatus.HasError &&
                    printerStatus.TonerLevel > _config.LowTonerThreshold)
                {
                    await ResolveExistingAlerts(printerStatus.PrinterId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluando estado de la impresora {PrinterId}", printerStatus.PrinterId);
            }
        }

        private async Task CreateAlertIfNotExists(PrinterStatus printerStatus, AlertType alertType, string description, AlertSeverity severity)
        {
            var existingAlerts = await _alertService.GetActiveAlertsByPrinterAsync(printerStatus.PrinterId);
            var existingAlert = existingAlerts.FirstOrDefault(a => a.Type == alertType);

            if (existingAlert == null)
            {
                var alertDto = new CreateAlertDTO
                {
                    PrinterId = printerStatus.PrinterId,
                    Type = alertType,
                    Description = description,
                    Severity = severity,
                    DetectedAt = DateTime.UtcNow
                };

                await _alertService.CreateAlertAsync(alertDto);
                _logger.LogWarning("Alerta creada: {PrinterId} - {AlertType}", printerStatus.PrinterId, alertType.ToString());
            }
        }

        private async Task ResolveExistingAlerts(int printerId)
        {
            var activeAlerts = await _alertService.GetActiveAlertsByPrinterAsync(printerId);
            
            foreach (var alert in activeAlerts)
            {
                var updateDto = new UpdateAlertDTO
                {
                    ResolvedAt = DateTime.UtcNow,
                    Status = AlertStatus.Resolved
                };

                await _alertService.UpdateAlertAsync(alert.Id, updateDto);
                _logger.LogInformation("Alerta resuelta: {AlertId} para impresora {PrinterId}", alert.Id, printerId);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class AlertEngineConfig
    {
        public int IntervalMinutes { get; set; } = 5;
        public int LowTonerThreshold { get; set; } = 10;
    }
}
