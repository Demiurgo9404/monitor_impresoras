using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using MonitorImpresoras.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class SnmpService : ISnmpService
    {
        private readonly int _timeout = 2000; // 2 segundos de tiempo de espera

        public async Task<Dictionary<string, string>> GetPrinterInfoAsync(string ipAddress, string community = "public")
        {
            var result = new Dictionary<string, string>();
            
            // OID para información de la impresora (ejemplos)
            var oids = new Dictionary<string, string>
            {
                { "sysDescr", ".1.3.6.1.2.1.1.1.0" },        // Descripción del sistema
                { "sysContact", ".1.3.6.1.2.1.1.4.0" },       // Contacto
                { "sysName", ".1.3.6.1.2.1.1.5.0" },          // Nombre del sistema
                { "sysLocation", ".1.3.6.1.2.1.1.6.0" },      // Ubicación
                { "prtSerialNumber", ".1.3.6.1.2.1.43.5.1.1.17.1" } // Número de serie
            };

            foreach (var oid in oids)
            {
                try
                {
                    var data = await GetSnmpDataAsync(ipAddress, oid.Value, community);
                    result[oid.Key] = data;
                }
                catch (Exception)
                {
                    result[oid.Key] = "No disponible";
                }
            }

            return result;
        }

        public async Task<int> GetPrinterPageCountAsync(string ipAddress, string community = "public")
        {
            // OID para el contador de páginas
            string pageCountOid = ".1.3.6.1.2.1.43.10.2.1.4.1.1";
            
            try
            {
                var result = await GetSnmpDataAsync(ipAddress, pageCountOid, community);
                if (int.TryParse(result, out int pageCount))
                {
                    return pageCount;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<int> GetTonerLevelAsync(string ipAddress, string color, string community = "public")
        {
            // OIDs para niveles de tóner (ejemplos para impresoras HP)
            var tonerOids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "black", ".1.3.6.1.2.1.43.11.1.1.9.1.1" },
                { "cyan", ".1.3.6.1.2.1.43.11.1.1.9.1.2" },
                { "magenta", ".1.3.6.1.2.1.43.11.1.1.9.1.3" },
                { "yellow", ".1.3.6.1.2.1.43.11.1.1.9.1.4" }
            };

            if (!tonerOids.TryGetValue(color.ToLower(), out string oid))
            {
                throw new ArgumentException($"Color no soportado: {color}");
            }

            try
            {
                var result = await GetSnmpDataAsync(ipAddress, oid, community);
                if (int.TryParse(result, out int level))
                {
                    return level;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task<bool> IsPrinterOnlineAsync(string ipAddress, int timeout = 1000)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    var task = tcpClient.ConnectAsync(ipAddress, 161); // Puerto SNMP
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                        return tcpClient.Connected;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GetSnmpDataAsync(string ipAddress, string oid, string community)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), 161);
            var communityData = new OctetString(community);
            var variables = new List<Variable> { new Variable(new ObjectIdentifier(oid)) };
            
            var result = await Messenger.GetAsync(
                version: VersionCode.V1,
                endpoint: endpoint,
                community: communityData,
                variables: variables,
                timeout: _timeout);

            return result[0].Data.ToString();
        }
    }
}
