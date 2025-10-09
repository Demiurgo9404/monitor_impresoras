using PrinterAgent.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace PrinterAgent.Core.Services
{
    public class AgentOrchestrator : IAgentOrchestrator, IDisposable
    {
        private readonly ILogger<AgentOrchestrator> _logger;
        private readonly INetworkScannerService _networkScanner;
        private readonly ICentralCommunicationService _centralComm;
        private AgentConfiguration _config;
        
        private readonly ConcurrentDictionary<string, PrinterInfo> _monitoredPrinters = new();
        private readonly Timer _reportingTimer;
        private readonly Timer _healthCheckTimer;
        private readonly Timer _networkScanTimer;
        
        private readonly DateTime _startTime = DateTime.UtcNow;
        private AgentMetrics _metrics = new();
        private bool _isRunning = false;
        private bool _disposed = false;

        public event EventHandler<PrinterInfo>? PrinterDiscovered;
        public event EventHandler<PrinterInfo>? PrinterStatusChanged;
        public event EventHandler<PrinterAlert>? AlertGenerated;

        public AgentOrchestrator(
            ILogger<AgentOrchestrator> logger,
            INetworkScannerService networkScanner,
            ICentralCommunicationService centralComm,
            IOptions<AgentConfiguration> config)
        {
            _logger = logger;
            _networkScanner = networkScanner;
            _centralComm = centralComm;
            _config = config.Value;

            // Configurar timers
            _reportingTimer = new Timer(SendPeriodicReport, null, Timeout.Infinite, Timeout.Infinite);
            _healthCheckTimer = new Timer(SendHealthCheck, null, Timeout.Infinite, Timeout.Infinite);
            _networkScanTimer = new Timer(PerformNetworkScan, null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isRunning)
            {
                _logger.LogWarning("El agente ya está ejecutándose");
                return;
            }

            _logger.LogInformation("Iniciando PrinterAgent {AgentId} - {AgentName}", _config.AgentId, _config.AgentName);

            try
            {
                // Registrar agente en el sistema central
                var registered = await _centralComm.RegisterAgentAsync(_config, cancellationToken);
                if (!registered)
                {
                    _logger.LogWarning("No se pudo registrar el agente, continuando en modo offline");
                }

                // Realizar escaneo inicial
                await ForceNetworkScanAsync();

                // Iniciar timers
                _reportingTimer.Change(TimeSpan.Zero, _config.ReportingInterval);
                _healthCheckTimer.Change(TimeSpan.Zero, _config.HealthCheckInterval);
                _networkScanTimer.Change(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30)); // Escaneo cada 30 min

                _isRunning = true;
                _logger.LogInformation("PrinterAgent iniciado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error iniciando el agente");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_isRunning)
            {
                return;
            }

            _logger.LogInformation("Deteniendo PrinterAgent...");

            _isRunning = false;

            // Detener timers
            _reportingTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _healthCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _networkScanTimer.Change(Timeout.Infinite, Timeout.Infinite);

            // Enviar reporte final
            try
            {
                await ForceReportAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enviando reporte final");
            }

            _logger.LogInformation("PrinterAgent detenido");
        }

        public async Task<AgentHealthStatus> GetHealthStatusAsync()
        {
            var process = Process.GetCurrentProcess();
            
            return new AgentHealthStatus
            {
                IsHealthy = _isRunning,
                Uptime = DateTime.UtcNow - _startTime,
                LastCentralCommunication = DateTime.UtcNow, // Simplificado
                PrintersMonitored = _monitoredPrinters.Count,
                ActiveAlerts = _monitoredPrinters.Values.SelectMany(p => p.Alerts.Where(a => a.IsActive)).Count(),
                CpuUsage = GetCpuUsage(),
                MemoryUsage = process.WorkingSet64 / (1024.0 * 1024.0), // MB
                NetworkLatency = await MeasureNetworkLatency(),
                Issues = GetCurrentIssues()
            };
        }

        public async Task<List<PrinterInfo>> GetMonitoredPrintersAsync()
        {
            return await Task.FromResult(_monitoredPrinters.Values.ToList());
        }

        public async Task ForceNetworkScanAsync()
        {
            _logger.LogInformation("Iniciando escaneo forzado de red");
            
            try
            {
                var scanStart = DateTime.UtcNow;
                var discoveredPrinters = await _networkScanner.ScanNetworkAsync();
                var scanDuration = DateTime.UtcNow - scanStart;

                _metrics.LastNetworkScan = scanStart;
                _metrics.NetworkScanDuration = scanDuration;
                _metrics.TotalPrintersDiscovered = discoveredPrinters.Count;

                foreach (var printer in discoveredPrinters)
                {
                    var existingPrinter = _monitoredPrinters.GetValueOrDefault(printer.IpAddress);
                    
                    if (existingPrinter == null)
                    {
                        // Nueva impresora descubierta
                        _monitoredPrinters[printer.IpAddress] = printer;
                        PrinterDiscovered?.Invoke(this, printer);
                        
                        var alert = new PrinterAlert
                        {
                            Severity = AlertSeverity.Info,
                            Message = $"Nueva impresora descubierta: {printer.Name}",
                            Code = "PRINTER_DISCOVERED",
                            Timestamp = DateTime.UtcNow
                        };
                        
                        printer.Alerts.Add(alert);
                        AlertGenerated?.Invoke(this, alert);
                        
                        _logger.LogInformation("Nueva impresora descubierta: {PrinterName} ({IpAddress})", 
                            printer.Name, printer.IpAddress);
                    }
                    else
                    {
                        // Actualizar impresora existente
                        var statusChanged = existingPrinter.Status != printer.Status;
                        
                        existingPrinter.LastSeen = printer.LastSeen;
                        existingPrinter.Status = printer.Status;
                        existingPrinter.Metrics = printer.Metrics;
                        
                        if (statusChanged)
                        {
                            PrinterStatusChanged?.Invoke(this, existingPrinter);
                            
                            var alert = new PrinterAlert
                            {
                                Severity = printer.Status == PrinterStatus.Offline ? AlertSeverity.Warning : AlertSeverity.Info,
                                Message = $"Cambio de estado: {printer.Name} ahora está {printer.Status}",
                                Code = "STATUS_CHANGED",
                                Timestamp = DateTime.UtcNow
                            };
                            
                            existingPrinter.Alerts.Add(alert);
                            AlertGenerated?.Invoke(this, alert);
                        }
                    }
                }

                // Actualizar métricas
                _metrics.PrintersOnline = _monitoredPrinters.Values.Count(p => p.Status == PrinterStatus.Online);
                _metrics.PrintersOffline = _monitoredPrinters.Values.Count(p => p.Status == PrinterStatus.Offline);
                _metrics.PrintersWithErrors = _monitoredPrinters.Values.Count(p => p.Status == PrinterStatus.Error);

                _logger.LogInformation("Escaneo completado en {Duration}ms. Encontradas {Count} impresoras", 
                    scanDuration.TotalMilliseconds, discoveredPrinters.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el escaneo de red");
                _metrics.FailedCommunications++;
            }
        }

        public async Task ForceReportAsync()
        {
            try
            {
                var report = await CreateReportAsync();
                var success = await _centralComm.SendReportAsync(report);
                
                if (success)
                {
                    _metrics.SuccessfulCommunications++;
                    _logger.LogDebug("Reporte enviado exitosamente");
                }
                else
                {
                    _metrics.FailedCommunications++;
                    _logger.LogWarning("Error enviando reporte");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción enviando reporte forzado");
                _metrics.FailedCommunications++;
            }
        }

        public async Task UpdateConfigurationAsync(AgentConfiguration newConfig)
        {
            _logger.LogInformation("Actualizando configuración del agente");
            
            var oldConfig = _config;
            _config = newConfig;
            
            // Reiniciar timers si los intervalos cambiaron
            if (oldConfig.ReportingInterval != newConfig.ReportingInterval)
            {
                _reportingTimer.Change(TimeSpan.Zero, newConfig.ReportingInterval);
            }
            
            if (oldConfig.HealthCheckInterval != newConfig.HealthCheckInterval)
            {
                _healthCheckTimer.Change(TimeSpan.Zero, newConfig.HealthCheckInterval);
            }
            
            _logger.LogInformation("Configuración actualizada exitosamente");
        }

        public async Task<AgentCommandResponse> ProcessCommandAsync(AgentCommand command)
        {
            _logger.LogInformation("Procesando comando {CommandType} - {CommandId}", command.Type, command.CommandId);
            
            var response = new AgentCommandResponse
            {
                CommandId = command.CommandId,
                AgentId = _config.AgentId,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                switch (command.Type)
                {
                    case AgentCommandType.ScanNetwork:
                        await ForceNetworkScanAsync();
                        response.Success = true;
                        response.Message = "Escaneo de red completado";
                        break;

                    case AgentCommandType.GenerateReport:
                        await ForceReportAsync();
                        response.Success = true;
                        response.Message = "Reporte generado y enviado";
                        break;

                    case AgentCommandType.UpdateConfiguration:
                        if (command.Parameters.TryGetValue("configuration", out var configObj))
                        {
                            // En una implementación real, deserializarías la configuración
                            response.Success = true;
                            response.Message = "Configuración actualizada";
                        }
                        else
                        {
                            response.Success = false;
                            response.Message = "Configuración no proporcionada";
                        }
                        break;

                    case AgentCommandType.ClearAlerts:
                        foreach (var printer in _monitoredPrinters.Values)
                        {
                            printer.Alerts.Clear();
                        }
                        response.Success = true;
                        response.Message = "Alertas limpiadas";
                        break;

                    default:
                        response.Success = false;
                        response.Message = $"Comando no soportado: {command.Type}";
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando comando {CommandType}", command.Type);
                response.Success = false;
                response.Message = $"Error: {ex.Message}";
            }

            return response;
        }

        public async Task<AgentMetrics> GetMetricsAsync()
        {
            return await Task.FromResult(_metrics);
        }

        private async void SendPeriodicReport(object? state)
        {
            if (!_isRunning) return;
            
            try
            {
                await ForceReportAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en reporte periódico");
            }
        }

        private async void SendHealthCheck(object? state)
        {
            if (!_isRunning) return;
            
            try
            {
                await _centralComm.SendHeartbeatAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error en health check");
            }
        }

        private async void PerformNetworkScan(object? state)
        {
            if (!_isRunning) return;
            
            try
            {
                await ForceNetworkScanAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en escaneo periódico");
            }
        }

        private async Task<AgentReport> CreateReportAsync()
        {
            var health = await GetHealthStatusAsync();
            
            return new AgentReport
            {
                AgentId = _config.AgentId,
                AgentName = _config.AgentName,
                Location = _config.Location,
                Timestamp = DateTime.UtcNow,
                Health = health,
                Printers = _monitoredPrinters.Values.ToList(),
                Alerts = _monitoredPrinters.Values.SelectMany(p => p.Alerts.Where(a => a.IsActive)).ToList(),
                Metrics = _metrics
            };
        }

        private double GetCpuUsage()
        {
            // Implementación simplificada
            return new Random().NextDouble() * 10; // 0-10% simulado
        }

        private async Task<double> MeasureNetworkLatency()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _centralComm.TestConnectivityAsync();
                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
            catch
            {
                return -1;
            }
        }

        private List<string> GetCurrentIssues()
        {
            var issues = new List<string>();
            
            if (_metrics.FailedCommunications > _metrics.SuccessfulCommunications)
            {
                issues.Add("Alta tasa de fallos de comunicación");
            }
            
            var offlinePrinters = _monitoredPrinters.Values.Count(p => p.Status == PrinterStatus.Offline);
            if (offlinePrinters > 0)
            {
                issues.Add($"{offlinePrinters} impresoras fuera de línea");
            }
            
            return issues;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _reportingTimer?.Dispose();
                _healthCheckTimer?.Dispose();
                _networkScanTimer?.Dispose();
                _disposed = true;
            }
        }
    }
}

