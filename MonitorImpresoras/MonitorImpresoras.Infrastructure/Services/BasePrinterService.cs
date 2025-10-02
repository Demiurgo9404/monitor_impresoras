using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Services
{
    public abstract class BasePrinterService
    {
        protected readonly ILogger _logger;
        protected const int DefaultTimeout = 2000; // 2 segundos
        protected const int DefaultRetries = 1;

        protected BasePrinterService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async Task<bool> IsPrinterOnlineAsync(string ipAddress, int timeout = DefaultTimeout)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, timeout);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al hacer ping a la impresora {IpAddress}", ipAddress);
                return false;
            }
        }

        protected async Task<Variable> GetSnmpValueAsync(
            string ipAddress, 
            string oid, 
            string community = "public", 
            int port = 161)
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                var communityBytes = new OctetString(community);
                var request = new GetRequestMessage(
                    Messenger.NextRequestId,
                    VersionCode.V2,
                    communityBytes,
                    new List<Variable> { new Variable(new ObjectIdentifier(oid)) });

                var response = await request.GetResponseAsync(endpoint);
                return response.Pdu().Variables[0];
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener valor SNMP de {IpAddress} para OID {OID}", ipAddress, oid);
                return null;
            }
        }

        protected async Task<Dictionary<string, Variable>> GetMultipleSnmpValuesAsync(
            string ipAddress,
            IEnumerable<string> oids,
            string community = "public",
            int port = 161)
        {
            var result = new Dictionary<string, Variable>();
            var variables = new List<Variable>();
            
            foreach (var oid in oids)
            {
                variables.Add(new Variable(new ObjectIdentifier(oid)));
            }

            try
            {
                var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                var communityBytes = new OctetString(community);
                var request = new GetRequestMessage(
                    Messenger.NextRequestId,
                    VersionCode.V2,
                    communityBytes,
                    variables);

                var response = await request.GetResponseAsync(endpoint);
                
                foreach (var variable in response.Pdu().Variables)
                {
                    result[variable.Id.ToString()] = variable;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener múltiples valores SNMP de {IpAddress}", ipAddress);
            }

            return result;
        }

        protected void UpdatePrinterStatus(Printer printer, bool isOnline, string status = "")
        {
            printer.IsOnline = isOnline;
            printer.LastChecked = DateTime.UtcNow;
            
            if (!isOnline)
            {
                printer.Status = "Offline";
                printer.LastError = "No se pudo conectar a la impresora";
                return;
            }

            if (!string.IsNullOrEmpty(status))
            {
                printer.Status = status;
            }
            else
            {
                printer.Status = isOnline ? "Online" : "Offline";
            }
            
            printer.LastSeen = DateTime.UtcNow;
        }
    }
}
