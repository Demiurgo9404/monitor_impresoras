using System.Net;
using System.Threading.Tasks;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio SNMP que maneja la comunicación con Impresoras
    /// </summary>
    public interface ISnmpService
    {
        /// <summary>
        /// Obtiene el estado actual de la Impresora
        /// </summary>
        Task<string> GetPrinterStatusAsync(string printerName, string ipAddress, int port = 161);
        
        /// <summary>
        /// Obtiene el nivel de tinta o tóner para un color específico
        /// </summary>
        Task<int> GetInkLevelAsync(string printerName, string ipAddress, string printerModel, int port = 161);
        
        /// <summary>
        /// Obtiene métricas detalladas de la Impresora
        /// </summary>
        Task<string> GetPrinterMetricsAsync(IPAddress ipAddress, string printerModel);
        
        /// <summary>
        /// Verifica si la Impresora está en línea
        /// </summary>
        Task<bool> IsPrinterOnlineAsync(IPAddress ipAddress, int port = 161);

        /// <summary>
        /// Obtiene el contador de páginas de la Impresora
        /// </summary>
        Task<int> GetPageCountAsync(string printerName, string ipAddress, int port = 161);
    }
}
