using System;

namespace QOPIQ.Infrastructure.Options
{
    /// <summary>
    /// Opciones de configuración para el monitoreo de impresoras
    /// </summary>
    public class PrinterMonitoringOptions
    {
        public const string SectionName = "PrinterMonitoring";
        
        /// <summary>
        /// Intervalo de monitoreo en minutos
        /// </summary>
        public int MonitoringIntervalMinutes { get; set; } = 5;
        
        /// <summary>
        /// Tiempo de espera en segundos para la verificación de estado de la impresora
        /// </summary>
        public int PrinterStatusTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Nivel de tóner bajo (porcentaje)
        /// </summary>
        public int LowTonerThreshold { get; set; } = 20;
        
        /// <summary>
        /// Nivel de tóner crítico (porcentaje)
        /// </summary>
        public int CriticalTonerThreshold { get; set; } = 10;
        
        /// <summary>
        /// Nivel de papel bajo (cantidad de hojas)
        /// </summary>
        public int LowPaperThreshold { get; set; } = 50;
        
        /// <summary>
        /// Habilitar notificaciones por correo electrónico
        /// </summary>
        public bool EnableEmailNotifications { get; set; } = true;
        
        /// <summary>
        /// Dirección de correo electrónico para notificaciones
        /// </summary>
        public string NotificationEmail { get; set; } = "soporte@qopiq.com";
        
        /// <summary>
        /// Tiempo de espera para reconexión en segundos
        /// </summary>
        public int ReconnectionTimeoutSeconds { get; set; } = 60;
        
        /// <summary>
        /// Número máximo de reintentos para verificar el estado de una impresora
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;
        
        /// <summary>
        /// Tiempo de espera entre reintentos en segundos
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 5;
        
        /// <summary>
        /// Indica si se debe habilitar el monitoreo automático
        /// </summary>
        public bool EnableAutomaticMonitoring { get; set; } = true;
    }
}

