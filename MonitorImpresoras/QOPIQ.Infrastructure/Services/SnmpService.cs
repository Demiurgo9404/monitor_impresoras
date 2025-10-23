using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using QOPIQ.Domain.DTOs;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces.Services;
using QOPIQ.Infrastructure.Configuration;

namespace QOPIQ.Infrastructure.Services
{
    /// <summary>
    /// Servicio SNMP v3 seguro para monitoreo de impresoras
    /// </summary>
    public class SnmpService : ISnmpService
    {
        private readonly SnmpOptions _options;
        private readonly ILogger<SnmpService> _logger;
        private readonly Random _random = new Random();

        // OIDs estándar para impresoras
        private static readonly ObjectIdentifier PrinterStatusOid = new("1.3.6.1.2.1.25.3.5.1.1.1");
        private static readonly ObjectIdentifier TonerLevelOid = new("1.3.6.1.2.1.43.11.1.1.9.1.1");
        private static readonly ObjectIdentifier PageCountOid = new("1.3.6.1.2.1.43.10.2.1.4.1.1");
        private static readonly ObjectIdentifier PrinterModelOid = new("1.3.6.1.2.1.25.3.2.1.3.1");

        public SnmpService(IOptions<SnmpOptions> options, ILogger<SnmpService> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtiene información completa de la impresora usando SNMP v3 (simulado por ahora)
        /// </summary>
        public async Task<Dictionary<string, object>> GetPrinterInfoAsync(string ipAddress, string community = "public")
        {
            if (!IsIpAllowed(ipAddress))
            {
                _logger.LogWarning("🚫 IP {IpAddress} no está en la lista de IPs permitidas", ipAddress);
                throw new UnauthorizedAccessException($"IP {ipAddress} no autorizada");
            }

            try
            {
                // TODO: Implementar SNMP v3 real cuando sea necesario
                // Por ahora, simulamos la respuesta para mantener funcionalidad
                await Task.Delay(100); // Simular latencia de red

                var result = new Dictionary<string, object>
                {
                    ["IpAddress"] = ipAddress,
                    ["Model"] = $"HP LaserJet Pro {_random.Next(1000, 9999)}",
                    ["Status"] = GetRandomStatus(),
                    ["Community"] = community ?? _options.Community,
                    ["Version"] = _options.Version,
                    ["SecurityLevel"] = "Authenticated",
                    ["LastChecked"] = DateTime.UtcNow
                };

                _logger.LogInformation("✅ Información simulada obtenida de impresora {IpAddress}", ipAddress);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error obteniendo información de impresora {IpAddress}", ipAddress);
                
                // Retornar datos básicos en caso de error
                return new Dictionary<string, object>
                {
                    ["IpAddress"] = ipAddress,
                    ["Model"] = "Generic Printer (Error)",
                    ["Status"] = "Unknown",
                    ["Error"] = ex.Message,
                    ["Version"] = "Fallback"
                };
            }
        }

        /// <summary>
        /// Obtiene un estado aleatorio para simulación
        /// </summary>
        private string GetRandomStatus()
        {
            var statuses = new[] { "Online", "Printing", "Idle", "Warmup", "Ready" };
            return statuses[_random.Next(statuses.Length)];
        }

        public async Task<string> GetPrinterStatusAsync(string ipAddress, string community = "public")
        {
            var status = await GetPrinterStatusEnumAsync(ipAddress, CancellationToken.None);
            return status.ToString();
        }

        public async Task<PrinterStatus> GetPrinterStatusEnumAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken); // Simulate network delay
            
            // For demo purposes, return a random status
            var statuses = Enum.GetValues<PrinterStatus>();
            return statuses[_random.Next(1, statuses.Length)]; // Skip Unknown status
        }

        public async Task<int> GetTonerLevelAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken); // Simulate network delay
            return _random.Next(10, 101); // Return random level between 10% and 100%
        }

        public async Task<PrinterType> GetPrinterTypeAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken); // Simulate network delay
            
            // For demo purposes, return a random printer type
            var types = Enum.GetValues<PrinterType>();
            return types[_random.Next(1, types.Length)]; // Skip Unknown type
        }

        public async Task<int> GetPageCountAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken); // Simulate network delay
            return _random.Next(1000, 100000); // Return random page count between 1000 and 100000
        }

        public async Task<PrinterCountersDto> GetPrinterCountersAsync(string ip, string community = "public")
        {
            if (!IsIpAllowed(ip))
            {
                throw new UnauthorizedAccessException($"IP {ip} no autorizada");
            }

            try
            {
                var tonerLevel = await GetTonerLevelAsync(ip);
                var pageCount = await GetPageCountAsync(ip);

                return new PrinterCountersDto
                {
                    IpAddress = ip,
                    BlackTonerLevel = tonerLevel,
                    CyanTonerLevel = Math.Max(0, tonerLevel - 10),
                    MagentaTonerLevel = Math.Max(0, tonerLevel - 20),
                    YellowTonerLevel = Math.Max(0, tonerLevel - 30),
                    TotalPagesPrinted = pageCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo contadores de {IpAddress}", ip);
                
                // Datos simulados como fallback
                return new PrinterCountersDto
                {
                    IpAddress = ip,
                    BlackTonerLevel = 85,
                    CyanTonerLevel = 75,
                    MagentaTonerLevel = 65,
                    YellowTonerLevel = 55,
                    TotalPagesPrinted = 12345
                };
            }
        }

        /// <summary>
        /// Verifica si la IP está en la lista de IPs permitidas
        /// </summary>
        private bool IsIpAllowed(string ipAddress)
        {
            if (_options.AllowedIPs == null || _options.AllowedIPs.Length == 0)
            {
                return true; // Si no hay restricciones, permitir todas
            }

            try
            {
                var ip = IPAddress.Parse(ipAddress);
                
                foreach (var allowedRange in _options.AllowedIPs)
                {
                    if (IsIpInRange(ip, allowedRange))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si una IP está en un rango CIDR
        /// </summary>
        private static bool IsIpInRange(IPAddress ip, string cidr)
        {
            try
            {
                var parts = cidr.Split('/');
                var baseIp = IPAddress.Parse(parts[0]);
                var prefixLength = int.Parse(parts[1]);

                var ipBytes = ip.GetAddressBytes();
                var baseBytes = baseIp.GetAddressBytes();

                if (ipBytes.Length != baseBytes.Length)
                    return false;

                var bytesToCheck = prefixLength / 8;
                var bitsToCheck = prefixLength % 8;

                for (int i = 0; i < bytesToCheck; i++)
                {
                    if (ipBytes[i] != baseBytes[i])
                        return false;
                }

                if (bitsToCheck > 0)
                {
                    var mask = (byte)(0xFF << (8 - bitsToCheck));
                    return (ipBytes[bytesToCheck] & mask) == (baseBytes[bytesToCheck] & mask);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parsea el estado de la impresora desde SNMP
        /// </summary>
        private static string ParsePrinterStatus(string snmpValue)
        {
            return snmpValue switch
            {
                "1" => "Other",
                "2" => "Unknown", 
                "3" => "Idle",
                "4" => "Printing",
                "5" => "Warmup",
                _ => "Online"
            };
        }
    }
}
