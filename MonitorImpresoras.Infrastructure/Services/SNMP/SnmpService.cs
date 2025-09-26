using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces.Services;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Infrastructure.Services.SNMP
{
    public class SnmpService : ISnmpService, IDisposable
    {
        private readonly ILogger<SnmpService> _logger;
        private bool _disposed = false;

        public SnmpService(ILogger<SnmpService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsPrinterOnlineAsync(IPAddress ipAddress, int timeoutMs = 2000)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, timeoutMs);
                return reply.Status == IPStatus.Success;
            }
            catch (PingException pex)
            {
                _logger.LogWarning(pex, "Error al hacer ping a {IpAddress}", ipAddress);
                return false;
            }
        }

        public async Task<Dictionary<string, string>> GetPrinterMetricsAsync(IPAddress ipAddress, string communityString = "public")
        {
            var result = new Dictionary<string, string>();
            
            // OIDs comunes para impresoras
            var oids = new Dictionary<string, string>
            {
                { "Modelo", ".1.3.6.1.2.1.1.1.0" },
                { "Ubicación", ".1.3.6.1.2.1.1.6.0" },
                { "Contacto", ".1.3.6.1.2.1.1.4.0" },
                { "Nombre", ".1.3.6.1.2.1.1.5.0" },
                { "TiempoActividad", ".1.3.6.1.2.1.1.3.0" },
                { "EstadoImpresora", ".1.3.6.1.2.1.25.3.5.1.1.1" },
                { "ContadorPaginas", ".1.3.6.1.2.1.43.10.2.1.4.1.1" },
                { "NivelToner", ".1.3.6.1.2.1.43.11.1.1.9.1.1" },
                { "NivelTonerMax", ".1.3.6.1.2.1.43.11.1.1.8.1.1" },
                { "EstadoToner", ".1.3.6.1.2.1.43.11.1.1.9.1.1" }
            };

            var variables = new List<Variable>();
            foreach (var oid in oids.Values)
            {
                variables.Add(new Variable(new ObjectIdentifier(oid)));
            }

            try
            {
                var response = await Messenger.GetAsync(
                    VersionCode.V1,
                    new IPEndPoint(ipAddress, 161),
                    new OctetString(communityString),
                    variables,
                    2000);

                int index = 0;
                foreach (var oid in oids)
                {
                    if (index < response.Count)
                    {
                        var value = response[index].Data.ToString();
                        result[oid.Key] = value;
                    }
                    index++;
                }

                // Procesar estado de la impresora
                if (result.ContainsKey("EstadoImpresora"))
                {
                    result["Estado"] = GetPrinterStatusDescription(result["EstadoImpresora"]);
                }

                // Calcular porcentaje de tóner si está disponible
                if (result.ContainsKey("NivelToner") && result.ContainsKey("NivelTonerMax"))
                {
                    if (int.TryParse(result["NivelToner"], out int nivel) &&
                        int.TryParse(result["NivelTonerMax"], out int maxNivel) &&
                        maxNivel > 0)
                    {
                        int porcentaje = (nivel * 100) / maxNivel;
                        result["PorcentajeToner"] = $"{porcentaje}%";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener métricas SNMP de {IpAddress}", ipAddress);
                throw new ApplicationException($"Error al obtener métricas SNMP: {ex.Message}", ex);
            }

            return result;
        }

        private string GetPrinterStatusDescription(string statusValue)
        {
            if (int.TryParse(statusValue, out int statusCode))
            {
                return statusCode switch
                {
                    1 => "Otro",
                    2 => "Desconocido",
                    3 => "Inactiva",
                    4 => "Conectando",
                    5 => "Servicio de impresión",
                    6 => "Procesando",
                    7 => "Lista",
                    8 => "Sin conexión",
                    _ => "Estado desconocido"
                };
            }
            return statusValue;
        }

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
                    // Liberar recursos administrados
                }
                _disposed = true;
            }
        }

        ~SnmpService()
        {
            Dispose(false);
        }
    }
}
