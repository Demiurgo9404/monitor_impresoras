using PrinterAgent.Core.Models;

namespace PrinterAgent.Core.Services
{
    /// <summary>
    /// Servicio para comunicación con el sistema central
    /// </summary>
    public interface ICentralCommunicationService
    {
        /// <summary>
        /// Registra el agente en el sistema central
        /// </summary>
        Task<bool> RegisterAgentAsync(AgentConfiguration config, CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía un reporte al sistema central
        /// </summary>
        Task<bool> SendReportAsync(AgentReport report, CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía una alerta inmediata al sistema central
        /// </summary>
        Task<bool> SendAlertAsync(PrinterAlert alert, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene comandos pendientes del sistema central
        /// </summary>
        Task<List<AgentCommand>> GetPendingCommandsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía respuesta a un comando
        /// </summary>
        Task<bool> SendCommandResponseAsync(AgentCommandResponse response, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene configuración actualizada del sistema central
        /// </summary>
        Task<AgentConfiguration?> GetUpdatedConfigurationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Envía heartbeat al sistema central
        /// </summary>
        Task<bool> SendHeartbeatAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica la conectividad con el sistema central
        /// </summary>
        Task<bool> TestConnectivityAsync(CancellationToken cancellationToken = default);
    }
}
