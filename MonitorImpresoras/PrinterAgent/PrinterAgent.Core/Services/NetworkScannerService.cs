using PrinterAgent.Core.Models;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PrinterAgent.Core.Services
{
    public class NetworkScannerService : INetworkScannerService
    {
        private readonly ILogger<NetworkScannerService> _logger;
        private readonly AgentConfiguration _config;

        public NetworkScannerService(
            ILogger<NetworkScannerService> logger,
            IOptions<AgentConfiguration> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        public async Task<List<PrinterInfo>> ScanNetworkAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando escaneo de red para {RangeCount} rangos", _config.Network.ScanRanges.Count);
            
            var allPrinters = new List<PrinterInfo>();
            var scanTasks = new List<Task<List<PrinterInfo>>>();

            foreach (var range in _config.Network.ScanRanges)
            {
                scanTasks.Add(ScanRangeAsync(range, cancellationToken));
            }

            var results = await Task.WhenAll(scanTasks);
            
            foreach (var printers in results)
            {
                allPrinters.AddRange(printers);
            }

            _logger.LogInformation("Escaneo completado. Encontradas {PrinterCount} impresoras", allPrinters.Count);
            return allPrinters;
        }

        public async Task<List<PrinterInfo>> ScanRangeAsync(string ipRange, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Escaneando rango: {IpRange}", ipRange);
            
            var printers = new List<PrinterInfo>();
            var ipAddresses = ParseIpRange(ipRange);
            
            var semaphore = new SemaphoreSlim(_config.Network.MaxConcurrentScans);
            var scanTasks = ipAddresses.Select(async ip =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var printer = await ScanSingleIpAsync(ip, cancellationToken);
                    return printer;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(scanTasks);
            printers.AddRange(results.Where(p => p != null)!);

            return printers;
        }

        public async Task<PrinterInfo?> ScanSingleIpAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            try
            {
                // Primero verificar si la IP responde
                if (!await IsHostAliveAsync(ipAddress, cancellationToken))
                {
                    return null;
                }

                // Verificar puertos comunes de impresoras
                var commonPorts = new[] { 9100, 631, 515, 161 }; // JetDirect, IPP, LPR, SNMP
                var openPorts = new List<int>();

                foreach (var port in commonPorts)
                {
                    if (await IsPortOpenAsync(ipAddress, port, cancellationToken))
                    {
                        openPorts.Add(port);
                    }
                }

                if (!openPorts.Any())
                {
                    return null;
                }

                // Si tiene puertos de impresora abiertos, obtener más detalles
                return await GetPrinterDetailsAsync(ipAddress, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error escaneando IP {IpAddress}", ipAddress);
                return null;
            }
        }

        public async Task<PrinterInfo?> GetPrinterDetailsAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            try
            {
                var printer = new PrinterInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    IpAddress = ipAddress,
                    FirstDetected = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow,
                    Status = PrinterStatus.Online
                };

                // Intentar obtener información via SNMP
                await TryGetSnmpInfoAsync(printer, cancellationToken);

                // Intentar obtener MAC address
                printer.MacAddress = await GetMacAddressAsync(ipAddress);

                // Si no pudimos obtener un nombre, usar la IP
                if (string.IsNullOrEmpty(printer.Name))
                {
                    printer.Name = $"Printer-{ipAddress.Replace(".", "-")}";
                }

                _logger.LogDebug("Impresora detectada: {PrinterName} en {IpAddress}", printer.Name, ipAddress);
                return printer;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error obteniendo detalles de impresora en {IpAddress}", ipAddress);
                return null;
            }
        }

        private async Task<bool> IsHostAliveAsync(string ipAddress, CancellationToken cancellationToken)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ipAddress, 2000);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> IsPortOpenAsync(string ipAddress, int port, CancellationToken cancellationToken)
        {
            try
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(ipAddress, port);
                var timeoutTask = Task.Delay(3000, cancellationToken);
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == connectTask && tcpClient.Connected)
                {
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task TryGetSnmpInfoAsync(PrinterInfo printer, CancellationToken cancellationToken)
        {
            try
            {
                // Implementación básica de SNMP
                // En una implementación real, usarías una librería SNMP como SharpSNMP
                
                // OIDs comunes para impresoras
                var systemNameOid = "1.3.6.1.2.1.1.5.0";
                var systemDescrOid = "1.3.6.1.2.1.1.1.0";
                
                // Simular obtención de datos SNMP
                await Task.Delay(100, cancellationToken);
                
                // En una implementación real, aquí harías las consultas SNMP
                printer.Capabilities.SupportsSnmp = true;
                
                // Datos simulados
                if (string.IsNullOrEmpty(printer.Name))
                {
                    printer.Name = $"HP LaserJet {printer.IpAddress.Split('.').Last()}";
                }
                
                printer.Model = "HP LaserJet Pro";
                printer.Manufacturer = "HP";
                printer.SerialNumber = $"SN{DateTime.Now.Ticks % 1000000}";
                
                // Métricas simuladas
                printer.Metrics.TotalPageCount = new Random().Next(1000, 50000);
                printer.Metrics.BlackToner = new ConsumableLevel { CurrentLevel = 75, MaxLevel = 100 };
                printer.Metrics.LastUpdated = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No se pudo obtener información SNMP para {IpAddress}", printer.IpAddress);
                printer.Capabilities.SupportsSnmp = false;
            }
        }

        private async Task<string> GetMacAddressAsync(string ipAddress)
        {
            try
            {
                // En Windows, podríamos usar ARP table
                // En una implementación real, usarías comandos del sistema o APIs específicas
                await Task.Delay(50);
                
                // MAC simulada
                var random = new Random();
                var mac = string.Join(":", 
                    Enumerable.Range(0, 6)
                    .Select(_ => random.Next(0, 256).ToString("X2")));
                
                return mac;
            }
            catch
            {
                return string.Empty;
            }
        }

        private List<string> ParseIpRange(string ipRange)
        {
            var ips = new List<string>();
            
            try
            {
                if (ipRange.Contains("/"))
                {
                    // CIDR notation (e.g., 192.168.1.0/24)
                    var parts = ipRange.Split('/');
                    var baseIp = IPAddress.Parse(parts[0]);
                    var prefixLength = int.Parse(parts[1]);
                    
                    var subnet = new IPNetwork(baseIp, prefixLength);
                    ips.AddRange(subnet.GetAllIPs().Select(ip => ip.ToString()));
                }
                else if (ipRange.Contains("-"))
                {
                    // Range notation (e.g., 192.168.1.1-192.168.1.254)
                    var parts = ipRange.Split('-');
                    var startIp = IPAddress.Parse(parts[0]);
                    var endIp = IPAddress.Parse(parts[1]);
                    
                    ips.AddRange(GetIpRange(startIp, endIp));
                }
                else
                {
                    // Single IP
                    ips.Add(ipRange);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parseando rango IP: {IpRange}", ipRange);
            }
            
            return ips;
        }

        private IEnumerable<string> GetIpRange(IPAddress startIp, IPAddress endIp)
        {
            var start = BitConverter.ToUInt32(startIp.GetAddressBytes().Reverse().ToArray(), 0);
            var end = BitConverter.ToUInt32(endIp.GetAddressBytes().Reverse().ToArray(), 0);
            
            for (uint i = start; i <= end; i++)
            {
                var bytes = BitConverter.GetBytes(i).Reverse().ToArray();
                yield return new IPAddress(bytes).ToString();
            }
        }
    }

    // Clase auxiliar para manejar subredes CIDR
    public class IPNetwork
    {
        private readonly IPAddress _network;
        private readonly int _prefixLength;

        public IPNetwork(IPAddress network, int prefixLength)
        {
            _network = network;
            _prefixLength = prefixLength;
        }

        public IEnumerable<IPAddress> GetAllIPs()
        {
            var hostBits = 32 - _prefixLength;
            var numberOfHosts = (uint)Math.Pow(2, hostBits) - 2; // Excluir network y broadcast
            
            var networkBytes = _network.GetAddressBytes();
            var networkInt = BitConverter.ToUInt32(networkBytes.Reverse().ToArray(), 0);
            
            for (uint i = 1; i <= numberOfHosts; i++)
            {
                var hostInt = networkInt + i;
                var hostBytes = BitConverter.GetBytes(hostInt).Reverse().ToArray();
                yield return new IPAddress(hostBytes);
            }
        }
    }
}
