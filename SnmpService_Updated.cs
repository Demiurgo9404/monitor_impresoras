using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.Infrastructure.Services.SNMP
{
    /// <summary>
    /// Implementación del servicio SNMP para la comunicación con impresoras
    /// </summary>
    public class SnmpService : ISnmpService, IDisposable
    {
        private readonly ILogger<SnmpService> _logger;
        private bool _disposed = false;
        private const string DefaultCommunity = "public";

        public SnmpService(ILogger<SnmpService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene el estado actual de la Impresora
        /// </summary>
        public async Task<string> GetPrinterStatusAsync(string printerName, string ipAddress, int port = 161)
        {
            try
            {
                if (!IPAddress.TryParse(ipAddress, out var ip))
                {
                    _logger.LogWarning("Dirección IP inválida: {IpAddress}", ipAddress);
                    return "Error: Dirección IP inválida";
                }

                var oid = new ObjectIdentifier(".1.3.6.1.2.1.25.3.5.1.1");
                var result = await GetAsync(ipAddress, new[] { oid }, DefaultCommunity, port);
                
                if (result == null || result.Count == 0)
                    return "Unknown";

                return ((int)result[0].Data.ToUInt16()) switch
                {
                    1 => "Other",
                    2 => "Unknown",
                    3 => "Idle",
                    4 => "Printing",
                    5 => "Warming up",
                    _ => "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la impresora {PrinterName} ({IpAddress})", printerName, ipAddress);
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Obtiene el nivel de tinta o tóner para un color específico
        /// </summary>
        public async Task<int> GetInkLevelAsync(string printerName, string ipAddress, string printerModel, int port = 161)
        {
            try
            {
                if (!IPAddress.TryParse(ipAddress, out var ip))
                {
                    _logger.LogWarning("Dirección IP inválida: {IpAddress}", ipAddress);
                    return 0;
                }

                // Obtener OID basado en el modelo de la impresora
                var oid = GetInkOidForPrinterModel(printerModel);
                if (string.IsNullOrEmpty(oid))
                {
                    _logger.LogWarning("No se encontró OID para el modelo de impresora: {PrinterModel}", printerModel);
                    return 0;
                }

                var result = await GetAsync(ipAddress, new[] { new ObjectIdentifier(oid) }, DefaultCommunity, port);
                
                if (result == null || result.Count == 0)
                    return 0;

                var level = result[0].Data.ToUInt16();
                return Math.Clamp(level, 0, 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el nivel de tinta para la impresora {PrinterName} ({IpAddress})", printerName, ipAddress);
                return 0;
            }
        }

        /// <summary>
        /// Obtiene métricas detalladas de la Impresora
        /// </summary>
        public async Task<string> GetPrinterMetricsAsync(IPAddress ipAddress, string printerModel)
        {
            try
            {
                var metrics = new Dictionary<string, string>
                {
                    ["IP"] = ipAddress.ToString(),
                    ["Model"] = printerModel,
                    ["Status"] = "Unknown",
                    ["LastChecked"] = DateTime.UtcNow.ToString("o")
                };

                // Obtener estado
                var statusOid = new ObjectIdentifier(".1.3.6.1.2.1.25.3.5.1.1");
                var statusResult = await GetAsync(ipAddress.ToString(), new[] { statusOid }, DefaultCommunity);
                if (statusResult != null && statusResult.Count > 0)
                {
                    var statusCode = (int)statusResult[0].Data.ToUInt16();
                    metrics["Status"] = statusCode switch
                    {
                        1 => "Other",
                        2 => "Unknown",
                        3 => "Idle",
                        4 => "Printing",
                        5 => "Warming up",
                        _ => "Unknown"
                    };
                }

                // Obtener contador de páginas
                var pageCountOid = new ObjectIdentifier(".1.3.6.1.2.1.43.10.2.1.4.1.1");
                var pageCountResult = await GetAsync(ipAddress.ToString(), new[] { pageCountOid }, DefaultCommunity);
                if (pageCountResult != null && pageCountResult.Count > 0)
                {
                    metrics["PageCount"] = pageCountResult[0].Data.ToUInt32().ToString();
                }

                return string.Join("; ", metrics.Select(kv => $"{kv.Key}: {kv.Value}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de la impresora {IpAddress}", ipAddress);
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Verifica si la Impresora está en línea
        /// </summary>
        public async Task<bool> IsPrinterOnlineAsync(IPAddress ipAddress, int port = 161)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, 2000);
                
                if (reply.Status != IPStatus.Success)
                {
                    _logger.LogInformation("La impresora {IpAddress} no responde al ping", ipAddress);
                    return false;
                }

                try
                {
                    var oid = new ObjectIdentifier(".1.3.6.1.2.1.1.1.0"); // sysDescr
                    var result = await GetAsync(ipAddress.ToString(), new[] { oid }, DefaultCommunity, port, 2000);
                    return result != null && result.Count > 0;
                }
                catch (Exception snmpEx)
                {
                    _logger.LogWarning(snmpEx, "Error en la consulta SNMP a la impresora {IpAddress}, pero responde al ping", ipAddress);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar el estado de la impresora {IpAddress}", ipAddress);
                return false;
            }
        }

        /// <summary>
        /// Obtiene el contador de páginas de la Impresora
        /// </summary>
        public async Task<int> GetPageCountAsync(string printerName, string ipAddress, int port = 161)
        {
            try
            {
                if (!IPAddress.TryParse(ipAddress, out var ip))
                {
                    _logger.LogWarning("Dirección IP inválida: {IpAddress}", ipAddress);
                    return 0;
                }

                var oid = new ObjectIdentifier(".1.3.6.1.2.1.43.10.2.1.4.1.1"); // prtMarkerLifeCount
                var result = await GetAsync(ipAddress, new[] { oid }, DefaultCommunity, port);
                
                if (result == null || result.Count == 0)
                    return 0;

                return (int)result[0].Data.ToUInt32();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el contador de páginas de la impresora {PrinterName} ({IpAddress})", printerName, ipAddress);
                return 0;
            }
        }

        #region Métodos auxiliares

        private string GetInkOidForPrinterModel(string printerModel)
        {
            // Mapeo de modelos de impresora a sus respectivos OIDs
            // Esto debería ser configurable en la base de datos o configuración
            var oidMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["HP LaserJet"] = ".1.3.6.1.2.1.43.11.1.1.9.1.1",
                ["Epson"] = ".1.3.6.1.2.1.43.11.1.1.9.1.2",
                ["Canon"] = ".1.3.6.1.2.1.43.11.1.1.9.1.3",
                // Agregar más mapeos según sea necesario
            };

            // Buscar el OID más apropiado para el modelo
            foreach (var mapping in oidMappings)
            {
                if (printerModel.IndexOf(mapping.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return mapping.Value;
                }
            }

            // Valor por defecto si no se encuentra un mapeo específico
            return ".1.3.6.1.2.1.43.11.1.1.9.1";
        }

        private async Task<IList<Variable>> GetAsync(string ipAddress, ObjectIdentifier[] oids, string communityString, int port = 161, int timeout = 5000)
        {
            try
            {
                if (!IPAddress.TryParse(ipAddress, out var ip))
                {
                    throw new ArgumentException("Dirección IP inválida", nameof(ipAddress));
                }

                var endpoint = new IPEndPoint(ip, port);
                var community = new OctetString(communityString);
                var variables = oids.Select(oid => new Variable(oid)).ToList();

                var result = await Messenger.GetAsync(
                    VersionCode.V1,
                    endpoint,
                    community,
                    variables,
                    timeout
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la comunicación SNMP con {IpAddress}:{Port}", ipAddress, port);
                throw;
            }
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Liberar recursos administrados aquí si los hay
                }
                _disposed = true;
            }
        }

        ~SnmpService()
        {
            Dispose(false);
        }

        #endregion
    }
}
