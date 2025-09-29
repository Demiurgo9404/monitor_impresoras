namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de estrategias avanzadas de base de datos
    /// </summary>
    public interface IDatabaseStrategiesService
    {
        /// <summary>
        /// Ejecuta configuración completa de estrategias avanzadas de base de datos
        /// </summary>
        Task<DatabaseOptimizationResult> ConfigureCompleteDatabaseStrategiesAsync();
    }
}
