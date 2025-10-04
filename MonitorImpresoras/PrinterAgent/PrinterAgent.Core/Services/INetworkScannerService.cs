using PrinterAgent.Core.Models;

namespace PrinterAgent.Core.Services
{
    /// <summary>
    /// Servicio para escanear la red y descubrir impresoras
    /// </summary>
    public interface INetworkScannerService
    {
        /// <summary>
        /// Escanea los rangos de red configurados en busca de impresoras
        /// </summary>
        Task<List<PrinterInfo>> ScanNetworkAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Escanea un rango específico de IPs
        /// </summary>
        Task<List<PrinterInfo>> ScanRangeAsync(string ipRange, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si una IP específica tiene una impresora
        /// </summary>
        Task<PrinterInfo?> ScanSingleIpAsync(string ipAddress, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene información detallada de una impresora conocida
        /// </summary>
        Task<PrinterInfo?> GetPrinterDetailsAsync(string ipAddress, CancellationToken cancellationToken = default);
    }
}
