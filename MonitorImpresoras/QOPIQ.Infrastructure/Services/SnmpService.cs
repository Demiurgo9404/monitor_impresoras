using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces;

namespace QOPIQ.Infrastructure.Services
{
    public class SnmpService : ISnmpService
    {
        private readonly ILogger<SnmpService> _logger;
        private const string CommunityString = "public"; // Debería venir de configuración
        private const int Timeout = 3000; // 3 segundos

        public SnmpService(ILogger<SnmpService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetPrinterStatusAsync(string ipAddress)
        {
            try
            {
                _logger.LogInformation("Obteniendo estado de la impresora en {IP}", ipAddress);
                
                // Verificar si la dirección IP es válida
                if (!IPAddress.TryParse(ipAddress, out _))
                {
                    _logger.LogWarning("La dirección IP {IP} no es válida", ipAddress);
                    return "Dirección IP no válida";
                }

                // OID para el estado de la impresora (ejemplo genérico)
                var oid = new ObjectIdentifier(".1.3.6.1.2.1.25.3.2.1.5.1");
                
                var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), 161);
                var result = await GetAsync(endpoint, oid);
                
                return InterpretPrinterStatus(result);
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Error de conexión al intentar comunicarse con la impresora en {IP}", ipAddress);
                return "Error de conexión";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de la impresora en {IP}", ipAddress);
                return "Error al obtener estado";
            }
        }

        private async Task<Variable> GetAsync(IPEndPoint endpoint, ObjectIdentifier oid)
        {
            var variables = new[] { new Variable(oid) };
            var result = await Messenger.GetAsync(
                VersionCode.V2,
                endpoint,
                new OctetString(CommunityString),
                variables,
                Timeout);
            
            return result[0];
        }

        private string InterpretPrinterStatus(Variable result)
        {
            if (result == null || result.Data == null)
                return "Estado desconocido";

            // Interpretar el estado según el valor devuelto
            // Estos valores pueden variar según el fabricante de la impresora
            return result.Data.ToString() switch
            {
                "3" => "Lista",
                "4" => "Imprimiendo",
                "5" => "En espera",
                "6" => "Procesando",
                "7" => "Apagada",
                "8" => "Fuera de línea",
                _ => "Estado desconocido"
            };
        }
    }
}
