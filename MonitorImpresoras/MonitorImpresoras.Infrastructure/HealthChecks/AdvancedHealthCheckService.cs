using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Interfaces;
using System.Net.NetworkInformation;

namespace MonitorImpresoras.Infrastructure.HealthChecks
{
    /// <summary>
    /// Health checks avanzados para servicios enterprise
    /// </summary>
    public class AdvancedHealthCheckService : IAdvancedHealthCheckService
    {
        private readonly ILogger<AdvancedHealthCheckService> _logger;
        private readonly IApplicationDbContext _dbContext;
        private readonly ITelemetryService _telemetryService;
        private readonly IRedisService _redisService;
        private readonly IPrinterStatusService _printerStatusService;

        public AdvancedHealthCheckService(
            ILogger<AdvancedHealthCheckService> logger,
            IApplicationDbContext dbContext,
            ITelemetryService telemetryService,
            IRedisService redisService,
            IPrinterStatusService printerStatusService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _telemetryService = telemetryService;
            _redisService = redisService;
            _printerStatusService = printerStatusService;
        }

        /// <summary>
        /// Ejecuta health checks completos del sistema
        /// </summary>
        public async Task<ComprehensiveHealthReport> ExecuteCompleteHealthCheckAsync()
        {
            try
            {
                _logger.LogInformation("Ejecutando health checks completos del sistema");

                var report = new ComprehensiveHealthReport
                {
                    CheckTimestamp = DateTime.UtcNow,
                    OverallStatus = HealthStatus.Healthy,
                    Components = new List<ComponentHealth>()
                };

                // 1. Health check de base de datos
                var dbHealth = await CheckDatabaseHealthAsync();
                report.Components.Add(dbHealth);

                // 2. Health check de Redis
                var redisHealth = await CheckRedisHealthAsync();
                report.Components.Add(redisHealth);

                // 3. Health check de servicios externos
                var externalHealth = await CheckExternalServicesHealthAsync();
                report.Components.Add(externalHealth);

                // 4. Health check de impresoras conectadas
                var printersHealth = await CheckPrintersConnectivityAsync();
                report.Components.Add(printersHealth);

                // 5. Health check de métricas del sistema
                var systemMetricsHealth = await CheckSystemMetricsHealthAsync();
                report.Components.Add(systemMetricsHealth);

                // 6. Health check de servicios de fondo
                var backgroundServicesHealth = await CheckBackgroundServicesHealthAsync();
                report.Components.Add(backgroundServicesHealth);

                // 7. Health check de memoria y recursos
                var memoryHealth = await CheckMemoryAndResourcesHealthAsync();
                report.Components.Add(memoryHealth);

                // 8. Health check de conectividad de red
                var networkHealth = await CheckNetworkConnectivityAsync();
                report.Components.Add(networkHealth);

                // Determinar estado general
                report.OverallStatus = report.Components.Any(c => c.Status == HealthStatus.Unhealthy)
                    ? HealthStatus.Unhealthy
                    : report.Components.Any(c => c.Status == HealthStatus.Degraded)
                    ? HealthStatus.Degraded
                    : HealthStatus.Healthy;

                // Generar recomendaciones
                report.Recommendations = GenerateHealthRecommendations(report.Components);

                _logger.LogInformation("Health checks completos ejecutados. Estado general: {Status}", report.OverallStatus);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando health checks completos");

                return new ComprehensiveHealthReport
                {
                    CheckTimestamp = DateTime.UtcNow,
                    OverallStatus = HealthStatus.Unhealthy,
                    Components = new List<ComponentHealth>(),
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// Verifica salud de la base de datos
        /// </summary>
        private async Task<ComponentHealth> CheckDatabaseHealthAsync()
        {
            var component = new ComponentHealth
            {
                Component = "Database",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                // 1. Verificar conexión básica
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    component.Status = HealthStatus.Unhealthy;
                    component.Error = "No se puede conectar a la base de datos";
                    return component;
                }

                // 2. Verificar tiempo de respuesta
                var startTime = DateTime.UtcNow;
                var printersCount = await _dbContext.Printers.CountAsync();
                var responseTime = DateTime.UtcNow - startTime;

                component.ResponseTime = responseTime.TotalMilliseconds;

                if (responseTime.TotalSeconds > 5)
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Tiempo de respuesta de BD elevado";
                }

                // 3. Verificar tablas críticas existen
                var hasPrintersTable = await _dbContext.Printers.AnyAsync();
                var hasTelemetryTable = await _dbContext.PrinterTelemetries.AnyAsync();

                component.Details = new Dictionary<string, object>
                {
                    ["PrintersCount"] = printersCount,
                    ["HasPrintersTable"] = hasPrintersTable,
                    ["HasTelemetryTable"] = hasTelemetryTable,
                    ["ResponseTimeMs"] = responseTime.TotalMilliseconds
                };

                _logger.LogDebug("Database health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en database health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica salud de Redis
        /// </summary>
        private async Task<ComponentHealth> CheckRedisHealthAsync()
        {
            var component = new ComponentHealth
            {
                Component = "Redis",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                var startTime = DateTime.UtcNow;

                // Verificar conexión y operaciones básicas
                var pingResult = await _redisService.PingAsync();
                var responseTime = DateTime.UtcNow - startTime;

                component.ResponseTime = responseTime.TotalMilliseconds;

                if (!pingResult)
                {
                    component.Status = HealthStatus.Unhealthy;
                    component.Error = "No se puede conectar a Redis";
                    return component;
                }

                if (responseTime.TotalSeconds > 1)
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Tiempo de respuesta de Redis elevado";
                }

                // Verificar operaciones de lectura/escritura
                var testKey = $"health_check_{DateTime.UtcNow.Ticks}";
                var testValue = "test_value";

                var setResult = await _redisService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var getResult = await _redisService.GetAsync(testKey);

                if (!setResult || getResult != testValue)
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Operaciones de lectura/escritura en Redis fallidas";
                }

                // Limpiar clave de prueba
                await _redisService.RemoveAsync(testKey);

                component.Details = new Dictionary<string, object>
                {
                    ["PingSuccessful"] = pingResult,
                    ["ResponseTimeMs"] = responseTime.TotalMilliseconds,
                    ["SetOperation"] = setResult,
                    ["GetOperation"] = getResult == testValue
                };

                _logger.LogDebug("Redis health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Redis health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica salud de servicios externos
        /// </summary>
        private async Task<ComponentHealth> CheckExternalServicesHealthAsync()
        {
            var component = new ComponentHealth
            {
                Component = "ExternalServices",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                var externalServices = new List<string> { "EmailService", "SmsService", "ExternalApi" };
                var healthyServices = new List<string>();
                var degradedServices = new List<string>();

                foreach (var service in externalServices)
                {
                    var serviceHealth = await CheckExternalServiceAsync(service);
                    if (serviceHealth.Status == HealthStatus.Healthy)
                    {
                        healthyServices.Add(service);
                    }
                    else if (serviceHealth.Status == HealthStatus.Degraded)
                    {
                        degradedServices.Add(service);
                    }
                    else
                    {
                        component.Status = HealthStatus.Unhealthy;
                        component.Error = $"Servicio externo {service} no disponible";
                    }
                }

                if (component.Status == HealthStatus.Healthy && degradedServices.Any())
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = $"Servicios externos degradados: {string.Join(", ", degradedServices)}";
                }

                component.Details = new Dictionary<string, object>
                {
                    ["HealthyServices"] = healthyServices,
                    ["DegradedServices"] = degradedServices,
                    ["TotalServices"] = externalServices.Count
                };

                _logger.LogDebug("External services health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en external services health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica conectividad de impresoras
        /// </summary>
        private async Task<ComponentHealth> CheckPrintersConnectivityAsync()
        {
            var component = new ComponentHealth
            {
                Component = "PrintersConnectivity",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                var printers = await _dbContext.Printers.Where(p => p.IsActive).ToListAsync();
                var onlinePrinters = 0;
                var offlinePrinters = 0;
                var errorPrinters = 0;

                foreach (var printer in printers)
                {
                    try
                    {
                        var telemetry = await _telemetryService.GetLatestTelemetryAsync(printer.Id);

                        if (telemetry?.IsOnline == true)
                        {
                            onlinePrinters++;
                        }
                        else if (telemetry?.Status == "Error")
                        {
                            errorPrinters++;
                        }
                        else
                        {
                            offlinePrinters++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error verificando conectividad de impresora {PrinterId}", printer.Id);
                        offlinePrinters++;
                    }
                }

                // Determinar estado basado en porcentajes
                var totalPrinters = printers.Count;
                if (totalPrinters == 0)
                {
                    component.Status = HealthStatus.Healthy;
                }
                else
                {
                    var offlinePercentage = (double)offlinePrinters / totalPrinters;
                    var errorPercentage = (double)errorPrinters / totalPrinters;

                    if (errorPercentage > 0.5 || offlinePercentage > 0.8)
                    {
                        component.Status = HealthStatus.Unhealthy;
                        component.Error = $"{errorPrinters} impresoras con error, {offlinePrinters} fuera de línea";
                    }
                    else if (errorPercentage > 0.2 || offlinePercentage > 0.5)
                    {
                        component.Status = HealthStatus.Degraded;
                        component.Warning = "Múltiples impresoras con problemas de conectividad";
                    }
                }

                component.Details = new Dictionary<string, object>
                {
                    ["TotalPrinters"] = totalPrinters,
                    ["OnlinePrinters"] = onlinePrinters,
                    ["OfflinePrinters"] = offlinePrinters,
                    ["ErrorPrinters"] = errorPrinters,
                    ["ConnectivityRate"] = totalPrinters > 0 ? (double)onlinePrinters / totalPrinters : 1.0
                };

                _logger.LogDebug("Printers connectivity health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en printers connectivity health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica métricas del sistema
        /// </summary>
        private async Task<ComponentHealth> CheckSystemMetricsHealthAsync()
        {
            var component = new ComponentHealth
            {
                Component = "SystemMetrics",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                var metrics = new Dictionary<string, object>();

                // Obtener métricas básicas del sistema
                metrics["ProcessorCount"] = Environment.ProcessorCount;
                metrics["TotalMemoryMB"] = GC.GetTotalMemory(false) / (1024 * 1024);
                metrics["WorkingSetMB"] = Environment.WorkingSet / (1024 * 1024);

                // Verificar uso de memoria
                var memoryUsageMB = (double)metrics["WorkingSetMB"];
                if (memoryUsageMB > 1024) // Más de 1GB
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Alto uso de memoria del proceso";
                }

                // Verificar uso de CPU (simulado)
                var cpuUsage = GetCpuUsage();
                metrics["CpuUsage"] = cpuUsage;

                if (cpuUsage > 80)
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Alto uso de CPU";
                }

                // Verificar espacio en disco
                var diskSpace = GetAvailableDiskSpace();
                metrics["AvailableDiskSpaceGB"] = diskSpace;

                if (diskSpace < 5) // Menos de 5GB disponibles
                {
                    component.Status = HealthStatus.Unhealthy;
                    component.Error = "Espacio en disco crítico";
                }

                component.Details = metrics;

                _logger.LogDebug("System metrics health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en system metrics health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica salud de servicios de fondo
        /// </summary>
        private async Task<ComponentHealth> CheckBackgroundServicesHealthAsync()
        {
            var component = new ComponentHealth
            {
                Component = "BackgroundServices",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                // Verificar que los servicios críticos estén corriendo
                var criticalServices = new[]
                {
                    "TelemetryCollectionJob",
                    "DailyReportJob",
                    "PrinterStatusCheckJob"
                };

                var runningServices = new List<string>();
                var stoppedServices = new List<string>();

                // Aquí verificarías el estado real de los servicios
                // Por ahora simulamos
                foreach (var service in criticalServices)
                {
                    var isRunning = await CheckServiceRunningAsync(service);
                    if (isRunning)
                    {
                        runningServices.Add(service);
                    }
                    else
                    {
                        stoppedServices.Add(service);
                    }
                }

                if (stoppedServices.Any())
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = $"Servicios detenidos: {string.Join(", ", stoppedServices)}";
                }

                component.Details = new Dictionary<string, object>
                {
                    ["RunningServices"] = runningServices,
                    ["StoppedServices"] = stoppedServices,
                    ["TotalServices"] = criticalServices.Length
                };

                _logger.LogDebug("Background services health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en background services health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica memoria y recursos del sistema
        /// </summary>
        private async Task<ComponentHealth> CheckMemoryAndResourcesHealthAsync()
        {
            var component = new ComponentHealth
            {
                Component = "MemoryAndResources",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                var process = Process.GetCurrentProcess();

                var memoryInfo = new Dictionary<string, object>
                {
                    ["ProcessMemoryMB"] = process.WorkingSet64 / (1024 * 1024),
                    ["VirtualMemoryMB"] = process.VirtualMemorySize64 / (1024 * 1024),
                    ["PrivateMemoryMB"] = process.PrivateMemorySize64 / (1024 * 1024),
                    ["PeakMemoryMB"] = process.PeakWorkingSet64 / (1024 * 1024),
                    ["ThreadCount"] = process.Threads.Count,
                    ["HandleCount"] = process.HandleCount
                };

                // Verificar límites críticos
                var processMemoryMB = (long)memoryInfo["ProcessMemoryMB"];
                if (processMemoryMB > 2048) // Más de 2GB
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Proceso usando mucha memoria";
                }

                var threadCount = (int)memoryInfo["ThreadCount"];
                if (threadCount > 100)
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Número elevado de threads";
                }

                component.Details = memoryInfo;

                _logger.LogDebug("Memory and resources health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en memory and resources health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        /// <summary>
        /// Verifica conectividad de red
        /// </summary>
        private async Task<ComponentHealth> CheckNetworkConnectivityAsync()
        {
            var component = new ComponentHealth
            {
                Component = "NetworkConnectivity",
                Status = HealthStatus.Healthy,
                CheckTime = DateTime.UtcNow
            };

            try
            {
                var networkInfo = new Dictionary<string, object>();

                // Verificar interfaces de red
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                var activeInterfaces = networkInterfaces.Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up).ToList();

                networkInfo["ActiveInterfaces"] = activeInterfaces.Count;
                networkInfo["TotalInterfaces"] = networkInterfaces.Length;

                if (activeInterfaces.Count == 0)
                {
                    component.Status = HealthStatus.Unhealthy;
                    component.Error = "No hay interfaces de red activas";
                }

                // Verificar conectividad a servicios críticos
                var canReachGoogle = await CanReachExternalServiceAsync("8.8.8.8");
                networkInfo["CanReachGoogleDNS"] = canReachGoogle;

                if (!canReachGoogle)
                {
                    component.Status = HealthStatus.Degraded;
                    component.Warning = "Problemas de conectividad externa";
                }

                component.Details = networkInfo;

                _logger.LogDebug("Network connectivity health check completado. Estado: {Status}", component.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en network connectivity health check");
                component.Status = HealthStatus.Unhealthy;
                component.Error = ex.Message;
            }

            return component;
        }

        // Métodos auxiliares

        private async Task<bool> CheckExternalServiceAsync(string serviceName)
        {
            // Implementar verificación específica por servicio
            return true; // Simulado
        }

        private async Task<bool> CheckServiceRunningAsync(string serviceName)
        {
            // Verificar si el servicio está corriendo
            return true; // Simulado
        }

        private double GetCpuUsage()
        {
            // Obtener uso de CPU (simulado)
            return 45.0; // 45%
        }

        private double GetAvailableDiskSpace()
        {
            // Obtener espacio disponible en disco (simulado)
            return 50.0; // 50GB
        }

        private async Task<bool> CanReachExternalServiceAsync(string host)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, 5000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private List<string> GenerateHealthRecommendations(List<ComponentHealth> components)
        {
            var recommendations = new List<string>();

            foreach (var component in components)
            {
                if (component.Status == HealthStatus.Unhealthy)
                {
                    switch (component.Component)
                    {
                        case "Database":
                            recommendations.Add("Verificar conexión a la base de datos y permisos");
                            recommendations.Add("Revisar logs de errores de base de datos");
                            break;
                        case "Redis":
                            recommendations.Add("Verificar configuración de Redis y conectividad");
                            recommendations.Add("Revisar memoria disponible en servidor Redis");
                            break;
                        case "PrintersConnectivity":
                            recommendations.Add("Verificar conectividad física de impresoras");
                            recommendations.Add("Revisar configuración de red de impresoras");
                            break;
                    }
                }
                else if (component.Status == HealthStatus.Degraded)
                {
                    switch (component.Component)
                    {
                        case "Database":
                            recommendations.Add("Considerar optimización de consultas lentas");
                            break;
                        case "MemoryAndResources":
                            recommendations.Add("Monitorear uso de memoria del proceso");
                            break;
                    }
                }
            }

            return recommendations;
        }
    }

    /// <summary>
    /// DTOs para health checks avanzados
    /// </summary>
    public class ComprehensiveHealthReport
    {
        public DateTime CheckTimestamp { get; set; }
        public HealthStatus OverallStatus { get; set; }
        public List<ComponentHealth> Components { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string? Error { get; set; }
    }

    public class ComponentHealth
    {
        public string Component { get; set; } = string.Empty;
        public HealthStatus Status { get; set; }
        public DateTime CheckTime { get; set; }
        public double? ResponseTime { get; set; }
        public string? Warning { get; set; }
        public string? Error { get; set; }
        public Dictionary<string, object>? Details { get; set; }
    }
}
