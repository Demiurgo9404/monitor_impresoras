namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para servicio de stress testing y validación de rendimiento
    /// </summary>
    public interface IStressTestingService
    {
        /// <summary>
        /// Ejecuta batería completa de stress tests para validar rendimiento enterprise
        /// </summary>
        Task<StressTestResult> RunCompleteStressTestSuiteAsync();
    }
}
