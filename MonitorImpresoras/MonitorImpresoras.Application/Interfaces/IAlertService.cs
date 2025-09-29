using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de alertas automáticas
    /// </summary>
    public interface IAlertService
    {
        /// <summary>
        /// Procesa el estado de una impresora y genera alertas si es necesario
        /// </summary>
        Task ProcessPrinterAlertAsync(Printer printer, PrinterStatus previousStatus);

        /// <summary>
        /// Procesa métricas del sistema y genera alertas si es necesario
        /// </summary>
        Task ProcessSystemMetricsAlertAsync(SystemMetrics metrics);

        /// <summary>
        /// Envía reporte diario consolidado
        /// </summary>
        Task SendDailySummaryReportAsync();
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
