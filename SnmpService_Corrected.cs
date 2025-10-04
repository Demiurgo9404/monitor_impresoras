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
using MonitorImpresoras.Application.Interfaces.Services;

namespace MonitorImpresoras.Infrastructure.Services.SNMP
{
    /// <summary>
    /// Implementación del servicio SNMP para la comunicación con impresoras
    /// </summary>
    public class SnmpService : ISnmpService, IDisposable
    {
        private readonly ILogger<SnmpService> _logger;
        private bool _disposed = false;

        public SnmpService(ILogger<SnmpService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene métricas de la impresora a través de SNMP
        /// </summary>
        /// <param name="ipAddress">Dirección IP de la impresora</param>
        /// <param name="communityString">Cadena de comunidad SNMP (por defecto: "public")</param>
        /// <returns>Diccionario con las métricas de la impresora</returns>
        public async Task<Dictionary<string, string>> GetPrinterMetricsAsync(IPAddress ipAddress, string communityString = "public")
        {
            try
            {
                var metrics = new Dictionary<string, string>();
                
                // Obtener estado de la impresora
                var statusOid = new ObjectIdentifier(".1.3.6.1.2.1.25.3.5.1.1");
                var statusResult = await GetAsync(ipAddress.ToString(), new[] { statusOid }, communityString);
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
                var pageCountResult = await GetAsync(ipAddress.ToString(), new[] { pageCountOid }, communityString);
                if (pageCountResult != null && pageCountResult.Count > 0)
                {
                    metrics["PageCount"] = pageCountResult[0].Data.ToUInt32().ToString();
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas de la impresora {IpAddress}", ipAddress);
                throw;
            }
        }

        /// <summary>
        /// Verifica si la impresora está en línea
        /// </summary>
        /// <param name="ipAddress">Dirección IP de la impresora</param>
        /// <param name="timeoutMs">Tiempo de espera en milisegundos (por defecto: 2000)</param>
        /// <returns>True si la impresora está en línea, false en caso contrario</returns>
        public async Task<bool> IsPrinterOnlineAsync(IPAddress ipAddress, int timeoutMs = 2000)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, timeoutMs);
                
                if (reply.Status != IPStatus.Success)
                {
                    _logger.LogInformation("La impresora {IpAddress} no responde al ping", ipAddress);
                    return false;
                }

                try
                {
                    var oid = new ObjectIdentifier(".1.3.6.1.2.1.1.1.0"); // sysDescr
                    var result = await GetAsync(ipAddress.ToString(), new[] { oid }, "public", 161, timeoutMs);
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

        #region Métodos auxiliares

        private async Task<IList<Variable>> GetAsync(string ipAddress, ObjectIdentifier[] oids, string communityString = "public", int port = 161, int timeout = 5000)
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
