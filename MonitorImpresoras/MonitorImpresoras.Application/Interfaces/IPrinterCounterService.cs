using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio para obtener contadores específicos de impresoras - QOPIQ
    /// </summary>
    public interface IPrinterCounterService
    {
        /// <summary>
        /// Obtiene todos los contadores de una impresora
        /// </summary>
        Task<PrinterCountersDto> GetPrinterCountersAsync(string ipAddress, string community = "public");

        /// <summary>
        /// Obtiene contadores de impresión por tamaño de papel
        /// </summary>
        Task<PrintCountersDto> GetPrintCountersAsync(string ipAddress, string community = "public");

        /// <summary>
        /// Obtiene contadores de scanner
        /// </summary>
        Task<ScanCountersDto> GetScanCountersAsync(string ipAddress, string community = "public");

        /// <summary>
        /// Obtiene estado de consumibles (toner, fusor, tambor)
        /// </summary>
        Task<ConsumablesStatusDto> GetConsumablesStatusAsync(string ipAddress, string community = "public");

        /// <summary>
        /// Obtiene información del fusor
        /// </summary>
        Task<FuserStatusDto> GetFuserStatusAsync(string ipAddress, string community = "public");

        /// <summary>
        /// Obtiene información del tambor/drum
        /// </summary>
        Task<DrumStatusDto> GetDrumStatusAsync(string ipAddress, string community = "public");

        /// <summary>
        /// Obtiene todos los datos en una sola consulta (optimizado)
        /// </summary>
        Task<CompletePrinterDataDto> GetCompletePrinterDataAsync(string ipAddress, string community = "public");
    }
}
