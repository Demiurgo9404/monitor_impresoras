using PrinterAgent.Core.Models;

namespace PrinterAgent.Core.Services
{
    /// <summary>
    /// Servicio principal que orquesta todas las operaciones del agente
    /// </summary>
    public interface IAgentOrchestrator
    {
        /// <summary>
        /// Inicia el agente
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Detiene el agente
        /// </summary>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el estado actual del agente
        /// </summary>
        Task<AgentHealthStatus> GetHealthStatusAsync();

        /// <summary>
        /// Obtiene la lista de impresoras monitoreadas
        /// </summary>
        Task<List<PrinterInfo>> GetMonitoredPrintersAsync();

        /// <summary>
        /// Fuerza un escaneo de red
        /// </summary>
        Task ForceNetworkScanAsync();

        /// <summary>
        /// Fuerza el envío de un reporte
        /// </summary>
        Task ForceReportAsync();

        /// <summary>
        /// Actualiza la configuración del agente
        /// </summary>
        Task UpdateConfigurationAsync(AgentConfiguration newConfig);

        /// <summary>
        /// Procesa un comando del sistema central
        /// </summary>
        Task<AgentCommandResponse> ProcessCommandAsync(AgentCommand command);

        /// <summary>
        /// Obtiene métricas del agente
        /// </summary>
        Task<AgentMetrics> GetMetricsAsync();

        /// <summary>
        /// Evento que se dispara cuando se detecta una nueva impresora
        /// </summary>
        event EventHandler<PrinterInfo> PrinterDiscovered;

        /// <summary>
        /// Evento que se dispara cuando cambia el estado de una impresora
        /// </summary>
        event EventHandler<PrinterInfo> PrinterStatusChanged;

        /// <summary>
        /// Evento que se dispara cuando se genera una alerta
        /// </summary>
        event EventHandler<PrinterAlert> AlertGenerated;
    }
}

