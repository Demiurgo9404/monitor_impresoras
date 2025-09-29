namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de configuración avanzada de IIS y Windows Server
    /// </summary>
    public interface IIisScalabilityService
    {
        /// <summary>
        /// Ejecuta configuración completa de optimización para IIS y Windows Server
        /// </summary>
        Task<IisOptimizationResult> ConfigureCompleteIisOptimizationAsync();
    }
}
