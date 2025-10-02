using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;

namespace MonitorImpresoras.Infrastructure.Services.SNMP
{
    public class SnmpService : ISnmpService, IDisposable
    {
        private readonly ILogger<SnmpService> _logger;
        private bool _disposed = false;

        public SnmpService(ILogger<SnmpService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetPrinterStatusAsync(string ipAddress, string communityString = "public", int port = 161)
        {
            try
            {
                // OID para el estado de la impresora (ejemplo: hrPrinterStatus)
                var oid = new ObjectIdentifier(".1.3.6.1.2.1.25.3.5.1.1");
                var result = await GetAsync(ipAddress, new[] { oid }, communityString, port);
                
                if (result == null || result.Count == 0)
                    return "Unknown";

                // Convertir el resultado a un estado legible
                // Los valores típicos son: 1=other, 2=unknown, 3=idle, 4=printing, 5=warmup
                return ((int)result[0].Data.ToUInt16()) switch
                {
                    1 => "Other",
                    2 => "Unknown",
                    3 => "Idle",
                    4 => "Printing",
                    5 => "Warmup",
                    _ => "Unknown"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la impresora {IpAddress}", ipAddress);
                return "Error";
            }
        }

        public async Task<int> GetInkLevelAsync(string ipAddress, string oid, string communityString = "public", int port = 161)
        {
            try
            {
                var result = await GetAsync(ipAddress, new[] { new ObjectIdentifier(oid) }, communityString, port);
                return result?[0]?.Data?.ToInt32() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el nivel de tinta de {IpAddress}", ipAddress);
                return -1; // Valor que indica error
            }
        }

        public async Task<int> GetTonerLevelAsync(string ipAddress, string oid, string communityString = "public", int port = 161)
        {
            try
            {
                var result = await GetAsync(ipAddress, new[] { new ObjectIdentifier(oid) }, communityString, port);
                return result?[0]?.Data?.ToInt32() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el nivel de tóner de {IpAddress}", ipAddress);
                return -1; // Valor que indica error
            }
        }

        public async Task<int> GetPageCountAsync(string ipAddress, string communityString = "public", int port = 161)
        {
            try
            {
                // OID para el contador de páginas (ejemplo: hrPrinterDetectedErrorState)
                var oid = new ObjectIdentifier(".1.3.6.1.2.1.25.3.5.1.2");
                var result = await GetAsync(ipAddress, new[] { oid }, communityString, port);
                return result?[0]?.Data?.ToInt32() ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el contador de páginas de {IpAddress}", ipAddress);
                return -1; // Valor que indica error
            }
        }

        private async Task<IList<Variable>> GetAsync(string ipAddress, IList<Variable> variables, string communityString, int port, int timeout = 5000)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
                ip = Dns.GetHostAddresses(ipAddress)[0];

            var endpoint = new IPEndPoint(ip, port);
            var community = new OctetString(communityString);
            var request = new GetRequestMessage(0, VersionCode.V2, community, variables);
            
            using var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = timeout,
                SendTimeout = timeout
            };

            var response = await request.GetResponseAsync(endpoint, client);
            return ((GetResponseMessage)response).Variables;
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
                    // Liberar recursos manejados
                }
                _disposed = true;
            }
        }
    }
}
