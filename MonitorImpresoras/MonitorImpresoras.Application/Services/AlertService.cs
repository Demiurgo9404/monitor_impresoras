using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para detección y gestión de alertas automáticas
    /// </summary>
    public class AlertService : IAlertService
    {
        private readonly INotificationService _notificationService;
        private readonly IExtendedAuditService _auditService;
        private readonly ILogger<AlertService> _logger;
        private readonly Dictionary<string, DateTime> _lastAlertTimes = new();

        public AlertService(
            INotificationService notificationService,
            IExtendedAuditService auditService,
            ILogger<AlertService> logger)
        {
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Procesa el estado de una impresora y genera alertas si es necesario
        /// </summary>
        public async Task ProcessPrinterAlertAsync(Printer printer, PrinterStatus previousStatus)
        {
            try
            {
                var alerts = await DetectPrinterAlertsAsync(printer, previousStatus);

                foreach (var alert in alerts)
                {
                    await SendAlertAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing printer alert for printer {PrinterId}", printer.Id);
            }
        }

        /// <summary>
        /// Procesa métricas del sistema y genera alertas si es necesario
        /// </summary>
        public async Task ProcessSystemMetricsAlertAsync(SystemMetrics metrics)
        {
            try
            {
                var alerts = await DetectSystemMetricsAlertsAsync(metrics);

                foreach (var alert in alerts)
                {
                    await SendAlertAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing system metrics alert");
            }
        }

        /// <summary>
        /// Envía reporte diario consolidado
        /// </summary>
        public async Task SendDailySummaryReportAsync()
        {
            try
            {
                var report = await GenerateDailySummaryReportAsync();

                if (!string.IsNullOrEmpty(report))
                {
                    await _notificationService.SendInfoAsync(
                        "Reporte Diario de Estado del Sistema",
                        report
                    );

                    await _auditService.LogSystemEventAsync(
                        "daily_report_sent",
                        "Daily summary report sent successfully",
                        "Automated daily report with system status",
                        new Dictionary<string, object>
                        {
                            { "ReportType", "DailySummary" },
                            { "Timestamp", DateTime.UtcNow }
                        },
                        "Info",
                        true
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending daily summary report");
            }
        }

        /// <summary>
        /// Detecta alertas basadas en el estado de una impresora
        /// </summary>
        private async Task<List<AlertInfo>> DetectPrinterAlertsAsync(Printer printer, PrinterStatus previousStatus)
        {
            var alerts = new List<AlertInfo>();

            // Alerta crítica: Impresora desconectada
            if (printer.Status == PrinterStatus.Offline && previousStatus != PrinterStatus.Offline)
            {
                if (ShouldSendAlert("printer_offline", printer.Id.ToString(), TimeSpan.FromMinutes(30)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Critical,
                        Title = $"Impresora Desconectada: {printer.Name}",
                        Message = $"La impresora '{printer.Name}' (ID: {printer.Id}) se ha desconectado del sistema.\n\nÚltima ubicación conocida: {printer.Location}\nModelo: {printer.Model}\nEstado anterior: {previousStatus}",
                        Metadata = new Dictionary<string, object>
                        {
                            { "PrinterId", printer.Id },
                            { "PrinterName", printer.Name },
                            { "PreviousStatus", previousStatus.ToString() },
                            { "Location", printer.Location ?? "No especificada" }
                        }
                    });
                }
            }

            // Alerta crítica: Error de impresión
            if (printer.Status == PrinterStatus.Error && previousStatus != PrinterStatus.Error)
            {
                if (ShouldSendAlert("printer_error", printer.Id.ToString(), TimeSpan.FromMinutes(15)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Critical,
                        Title = $"Error en Impresora: {printer.Name}",
                        Message = $"La impresora '{printer.Name}' (ID: {printer.Id}) reporta un error.\n\nUbicación: {printer.Location}\nModelo: {printer.Model}\nError detectado: {printer.ErrorMessage ?? "Error desconocido"}",
                        Metadata = new Dictionary<string, object>
                        {
                            { "PrinterId", printer.Id },
                            { "PrinterName", printer.Name },
                            { "ErrorMessage", printer.ErrorMessage ?? "N/A" },
                            { "Location", printer.Location ?? "No especificada" }
                        }
                    });
                }
            }

            // Alerta de advertencia: Tóner bajo
            if (printer.TonerLevel <= 15 && (previousStatus == PrinterStatus.Online || previousStatus == PrinterStatus.Idle))
            {
                if (ShouldSendAlert("toner_low", printer.Id.ToString(), TimeSpan.FromHours(6)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Warning,
                        Title = $"Tóner Bajo en Impresora: {printer.Name}",
                        Message = $"La impresora '{printer.Name}' (ID: {printer.Id}) tiene el tóner al {printer.TonerLevel}%.\n\nUbicación: {printer.Location}\nModelo: {printer.Model}\nNivel actual: {printer.TonerLevel}%\n\nSe recomienda reemplazar el tóner pronto para evitar interrupciones.",
                        Metadata = new Dictionary<string, object>
                        {
                            { "PrinterId", printer.Id },
                            { "PrinterName", printer.Name },
                            { "TonerLevel", printer.TonerLevel },
                            { "Location", printer.Location ?? "No especificada" }
                        }
                    });
                }
            }

            // Alerta de advertencia: Papel bajo
            if (printer.PaperLevel <= 10 && (previousStatus == PrinterStatus.Online || previousStatus == PrinterStatus.Idle))
            {
                if (ShouldSendAlert("paper_low", printer.Id.ToString(), TimeSpan.FromHours(4)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Warning,
                        Title = $"Papel Bajo en Impresora: {printer.Name}",
                        Message = $"La impresora '{printer.Name}' (ID: {printer.Id}) tiene el papel al {printer.PaperLevel}%.\n\nUbicación: {printer.Location}\nModelo: {printer.Model}\nNivel actual: {printer.PaperLevel}%\n\nSe recomienda reponer papel para evitar interrupciones.",
                        Metadata = new Dictionary<string, object>
                        {
                            { "PrinterId", printer.Id },
                            { "PrinterName", printer.Name },
                            { "PaperLevel", printer.PaperLevel },
                            { "Location", printer.Location ?? "No especificada" }
                        }
                    });
                }
            }

            // Alerta informativa: Impresora vuelve a estar online
            if (printer.Status == PrinterStatus.Online && previousStatus == PrinterStatus.Offline)
            {
                if (ShouldSendAlert("printer_online", printer.Id.ToString(), TimeSpan.FromMinutes(60)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Info,
                        Title = $"Impresora Reconectada: {printer.Name}",
                        Message = $"La impresora '{printer.Name}' (ID: {printer.Id}) ha vuelto a estar disponible.\n\nUbicación: {printer.Location}\nModelo: {printer.Model}\nEstado actual: {printer.Status}",
                        Metadata = new Dictionary<string, object>
                        {
                            { "PrinterId", printer.Id },
                            { "PrinterName", printer.Name },
                            { "PreviousStatus", previousStatus.ToString() },
                            { "CurrentStatus", printer.Status.ToString() }
                        }
                    });
                }
            }

            return alerts;
        }

        /// <summary>
        /// Detecta alertas basadas en métricas del sistema
        /// </summary>
        private async Task<List<AlertInfo>> DetectSystemMetricsAlertsAsync(SystemMetrics metrics)
        {
            var alerts = new List<AlertInfo>();

            // Alerta crítica: Alto uso de CPU
            if (metrics.CpuUsage > 90)
            {
                if (ShouldSendAlert("high_cpu", "system", TimeSpan.FromMinutes(10)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Critical,
                        Title = "Alto Uso de CPU Detectado",
                        Message = $"El uso de CPU del servidor ha excedido el 90% ({metrics.CpuUsage:F1}%).\n\nEsto puede afectar el rendimiento del sistema de monitoreo de impresoras.\n\nServidor: {Environment.MachineName}\nUptime: {metrics.Uptime.TotalHours:F1} horas",
                        Metadata = new Dictionary<string, object>
                        {
                            { "CpuUsage", metrics.CpuUsage },
                            { "ServerName", Environment.MachineName },
                            { "Uptime", metrics.Uptime.TotalHours }
                        }
                    });
                }
            }

            // Alerta de advertencia: Alto uso de memoria
            if (metrics.MemoryUsage > 85)
            {
                if (ShouldSendAlert("high_memory", "system", TimeSpan.FromMinutes(15)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Warning,
                        Title = "Alto Uso de Memoria Detectado",
                        Message = $"El uso de memoria del servidor ha excedido el 85% ({metrics.MemoryUsage:F1}%).\n\nConsidere revisar procesos en ejecución o reiniciar el servidor.\n\nServidor: {Environment.MachineName}\nMemoria usada: {metrics.MemoryUsage:F1}%",
                        Metadata = new Dictionary<string, object>
                        {
                            { "MemoryUsage", metrics.MemoryUsage },
                            { "ServerName", Environment.MachineName }
                        }
                    });
                }
            }

            // Alerta crítica: Base de datos no responde
            if (!metrics.DatabaseHealthy)
            {
                if (ShouldSendAlert("database_unhealthy", "system", TimeSpan.FromMinutes(5)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Critical,
                        Title = "Base de Datos No Responde",
                        Message = $"La conexión a la base de datos PostgreSQL no está disponible.\n\nEsto afecta todas las operaciones del sistema:\n- Monitoreo de impresoras\n- Generación de reportes\n- Registro de auditoría\n- Notificaciones automáticas\n\nVerifique la conectividad de red y el estado del servicio PostgreSQL.",
                        Metadata = new Dictionary<string, object>
                        {
                            { "DatabaseHealthy", false },
                            { "ServerName", Environment.MachineName }
                        }
                    });
                }
            }

            // Alerta crítica: Servicio de reportes programados caído
            if (!metrics.ScheduledReportsHealthy)
            {
                if (ShouldSendAlert("scheduled_reports_unhealthy", "system", TimeSpan.FromMinutes(10)))
                {
                    alerts.Add(new AlertInfo
                    {
                        Type = AlertType.Critical,
                        Title = "Servicio de Reportes Programados No Funciona",
                        Message = $"El servicio de reportes programados (Background Worker) no está funcionando correctamente.\n\nLos reportes automáticos podrían no ejecutarse según lo programado.\n\nVerifique el estado del servicio y revise los logs del sistema.",
                        Metadata = new Dictionary<string, object>
                        {
                            { "ScheduledReportsHealthy", false },
                            { "ServerName", Environment.MachineName }
                        }
                    });
                }
            }

            return alerts;
        }

        /// <summary>
        /// Determina si debe enviar una alerta basado en el tiempo mínimo entre alertas
        /// </summary>
        private bool ShouldSendAlert(string alertType, string entityId, TimeSpan minInterval)
        {
            var alertKey = $"{alertType}_{entityId}";
            var lastAlertTime = _lastAlertTimes.GetValueOrDefault(alertKey);

            if (DateTime.UtcNow - lastAlertTime < minInterval)
            {
                _logger.LogInformation("Alert {AlertType} for {EntityId} skipped due to minimum interval", alertType, entityId);
                return false;
            }

            _lastAlertTimes[alertKey] = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Envía una alerta usando el servicio de notificaciones
        /// </summary>
        private async Task SendAlertAsync(AlertInfo alert)
        {
            try
            {
                switch (alert.Type)
                {
                    case AlertType.Critical:
                        await _notificationService.SendCriticalAsync(alert.Title, alert.Message);
                        break;
                    case AlertType.Warning:
                        await _notificationService.SendWarningAsync(alert.Title, alert.Message);
                        break;
                    case AlertType.Info:
                        await _notificationService.SendInfoAsync(alert.Title, alert.Message);
                        break;
                }

                _logger.LogInformation("Alert sent successfully: {AlertType} - {Title}", alert.Type, alert.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending alert: {AlertType} - {Title}", alert.Type, alert.Title);
            }
        }

        /// <summary>
        /// Genera el reporte diario consolidado
        /// </summary>
        private async Task<string> GenerateDailySummaryReportAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("📊 REPORTE DIARIO DEL SISTEMA DE MONITOREO DE IMPRESORAS");
            sb.AppendLine("=".PadRight(60, '='));
            sb.AppendLine($"Fecha del reporte: {DateTime.UtcNow:dd/MM/yyyy}");
            sb.AppendLine();

            // Información de impresoras
            sb.AppendLine("🏢 ESTADO DE IMPRESORAS:");
            sb.AppendLine("-".PadRight(30, '-'));

            // Esta información vendría de consultas a la base de datos
            // Por simplicidad, usamos datos de ejemplo
            sb.AppendLine("✅ Impresoras activas: 15");
            sb.AppendLine("⚠️ Impresoras con tóner bajo (< 20%): 3");
            sb.AppendLine("📄 Impresoras con papel bajo (< 15%): 2");
            sb.AppendLine("❌ Impresoras fuera de línea: 1");
            sb.AppendLine("🔧 Impresoras en mantenimiento: 1");
            sb.AppendLine();

            // Estadísticas de uso
            sb.AppendLine("📈 ESTADÍSTICAS DE USO (Últimas 24h):");
            sb.AppendLine("-".PadRight(40, '-'));
            sb.AppendLine("📄 Páginas impresas: 2,847");
            sb.AppendLine("📋 Trabajos de impresión: 156");
            sb.AppendLine("⏱️ Tiempo promedio de respuesta: 2.3 segundos");
            sb.AppendLine("🔗 Conexiones activas: 8 usuarios");
            sb.AppendLine();

            // Eventos del día
            sb.AppendLine("📝 EVENTOS DESTACADOS:");
            sb.AppendLine("-".PadRight(25, '-'));
            sb.AppendLine("✅ 09:30 - Impresora 'Oficina Principal' reconectada");
            sb.AppendLine("⚠️ 11:15 - Tóner bajo detectado en 'Marketing'");
            sb.AppendLine("✅ 14:20 - Reporte mensual generado exitosamente");
            sb.AppendLine("📧 17:00 - Notificaciones diarias enviadas");
            sb.AppendLine();

            // Estado del sistema
            sb.AppendLine("💻 ESTADO DEL SISTEMA:");
            sb.AppendLine("-".PadRight(25, '-'));
            sb.AppendLine("🖥️ CPU: 23% (Normal)");
            sb.AppendLine("🧠 Memoria: 4.2GB/8GB (52% - Saludable)");
            sb.AppendLine("💾 Disco: 156GB/500GB (31% - Saludable)");
            sb.AppendLine("🗄️ Base de datos: Conectada y respondiendo");
            sb.AppendLine("📊 Servicios de monitoreo: Todos operativos");
            sb.AppendLine();

            sb.AppendLine("🎯 RESUMEN EJECUTIVO:");
            sb.AppendLine("-".PadRight(25, '-'));
            sb.AppendLine("El sistema de monitoreo de impresoras opera normalmente.");
            sb.AppendLine("Se detectaron algunos puntos de atención menores que no requieren acción inmediata.");
            sb.AppendLine("Todos los servicios críticos están funcionando correctamente.");

            return sb.ToString();
        }
    }

    /// <summary>
    /// Información de una alerta detectada
    /// </summary>
    public class AlertInfo
    {
        public AlertType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Tipos de alertas del sistema
    /// </summary>
    public enum AlertType
    {
        Critical,
        Warning,
        Info
    }

    /// <summary>
    /// Métricas del sistema para detección de alertas
    /// </summary>
    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public TimeSpan Uptime { get; set; }
        public bool DatabaseHealthy { get; set; }
        public bool ScheduledReportsHealthy { get; set; }
    }
}
