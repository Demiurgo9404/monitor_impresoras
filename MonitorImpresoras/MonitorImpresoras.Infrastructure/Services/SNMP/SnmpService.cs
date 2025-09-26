using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Formats.Asn1;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Infrastructure.Services.SNMP
{
    /// <summary>
    /// Servicio para monitoreo de impresoras vía SNMP
    /// </summary>
    public class SnmpService : ISnmpService
    {
        private const string SNMP_OID_PRINTER_STATUS = "1.3.6.1.2.1.25.3.5.1.1"; // hrPrinterStatus
        private const string SNMP_OID_DEVICE_STATUS = "1.3.6.1.2.1.25.3.2.1.5"; // hrDeviceStatus
        private const string SNMP_OID_PAGE_COUNT = "1.3.6.1.2.1.43.10.2.1.4"; // prtMarkerLifeCount
        private const string SNMP_OID_TONER_LEVEL = "1.3.6.1.2.1.43.11.1.1.9"; // prtMarkerSuppliesLevel
        private const string SNMP_OID_TONER_MAX = "1.3.6.1.2.1.43.11.1.1.8"; // prtMarkerSuppliesMaxCapacity

        /// <summary>
        /// Obtiene el estado de una impresora vía SNMP
        /// </summary>
        /// <param name="ipAddress">Dirección IP de la impresora</param>
        /// <param name="community">Community string SNMP (por defecto "public")</param>
        /// <param name="port">Puerto SNMP (por defecto 161)</param>
        /// <returns>Estado de la impresora</returns>
        public async Task<PrinterStatusInfo> GetPrinterStatusAsync(string ipAddress, string community = "public", int port = 161)
        {
            var status = new PrinterStatusInfo
            {
                IpAddress = ipAddress,
                LastUpdate = DateTime.UtcNow,
                IsOnline = false,
                Status = "Desconocido"
            };

            try
            {
                // Intentar obtener el estado general del dispositivo
                var deviceStatus = await GetSnmpValueAsync(ipAddress, community, port, SNMP_OID_DEVICE_STATUS);

                if (deviceStatus != null)
                {
                    status.IsOnline = true;
                    status.Status = ParseDeviceStatus(deviceStatus);
                }

                // Intentar obtener el estado específico de la impresora
                var printerStatus = await GetSnmpValueAsync(ipAddress, community, port, SNMP_OID_PRINTER_STATUS);

                if (printerStatus != null)
                {
                    status.Status = ParsePrinterStatus(printerStatus);
                }

                // Obtener contadores de páginas
                var pageCount = await GetSnmpValueAsync(ipAddress, community, port, SNMP_OID_PAGE_COUNT);
                if (pageCount != null)
                {
                    status.TonerLevel = int.Parse(pageCount); // Usando TonerLevel como TotalPages
                }

                // Obtener nivel de tóner
                var tonerLevel = await GetSnmpValueAsync(ipAddress, community, port, SNMP_OID_TONER_LEVEL);
                var tonerMax = await GetSnmpValueAsync(ipAddress, community, port, SNMP_OID_TONER_MAX);

                if (tonerLevel != null && tonerMax != null)
                {
                    status.PaperLevel = (int.Parse(tonerLevel) * 100) / int.Parse(tonerMax);
                }
            }
            catch (Exception ex)
            {
                status.IsOnline = false;
                status.Status = $"Error de conexión: {ex.Message}";
            }

            return status;
        }

        /// <summary>
        /// Obtiene el valor de un OID SNMP específico
        /// </summary>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="community">Community string</param>
        /// <param name="port">Puerto</param>
        /// <param name="oid">OID a consultar</param>
        /// <returns>Valor del OID o null si no se puede obtener</returns>
        private async Task<string?> GetSnmpValueAsync(string ipAddress, string community, int port, string oid)
        {
            try
            {
                using var udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

                var request = CreateSnmpGetRequest(community, oid);
                var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

                await udpClient.SendAsync(request, request.Length, endpoint);

                var result = await udpClient.ReceiveAsync();
                return ParseSnmpResponse(result.Buffer);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Crea una petición SNMP GET
        /// </summary>
        /// <param name="community">Community string</param>
        /// <param name="oid">OID a consultar</param>
        /// <returns>Array de bytes con la petición</returns>
        private byte[] CreateSnmpGetRequest(string community, string oid)
        {
            var request = new byte[1024];
            int offset = 0;

            // SNMP Version 1
            request[offset++] = 0x30; // SEQUENCE
            request[offset++] = 0x2b; // Length

            // Community string
            request[offset++] = 0x02; // INTEGER (version)
            request[offset++] = 0x01; // Length
            request[offset++] = 0x00; // SNMP v1

            request[offset++] = 0x04; // OCTET STRING (community)
            var communityBytes = Encoding.ASCII.GetBytes(community);
            request[offset++] = (byte)communityBytes.Length;
            Array.Copy(communityBytes, 0, request, offset, communityBytes.Length);
            offset += communityBytes.Length;

            // PDU (GET REQUEST)
            request[offset++] = 0xa0; // GET REQUEST
            request[offset++] = 0x1c; // Length

            request[offset++] = 0x02; // Request ID
            request[offset++] = 0x04;
            request[offset++] = 0x00;
            request[offset++] = 0x00;
            request[offset++] = 0x00;
            request[offset++] = 0x01;

            request[offset++] = 0x02; // Error status
            request[offset++] = 0x01;
            request[offset++] = 0x00;

            request[offset++] = 0x02; // Error index
            request[offset++] = 0x01;
            request[offset++] = 0x00;

            // Variable bindings
            request[offset++] = 0x30; // SEQUENCE
            request[offset++] = 0x0e; // Length

            // OID
            var oidBytes = ParseOidToBytes(oid);
            request[offset++] = 0x06; // OBJECT IDENTIFIER
            request[offset++] = (byte)oidBytes.Length;
            Array.Copy(oidBytes, 0, request, offset, oidBytes.Length);
            offset += oidBytes.Length;

            request[offset++] = 0x05; // NULL
            request[offset++] = 0x00;

            return request.Take(offset).ToArray();
        }

        /// <summary>
        /// Parsea una respuesta SNMP
        /// </summary>
        /// <param name="response">Array de bytes de respuesta</param>
        /// <returns>Valor del OID o null</returns>
        private string? ParseSnmpResponse(byte[] response)
        {
            try
            {
                // Implementación simplificada - en producción usar una librería SNMP completa
                // Buscar el valor en la respuesta
                for (int i = 0; i < response.Length - 2; i++)
                {
                    if (response[i] == 0x04 || response[i] == 0x02) // OCTET STRING o INTEGER
                    {
                        int length = response[i + 1];
                        if (i + 2 + length <= response.Length)
                        {
                            return Encoding.ASCII.GetString(response, i + 2, length);
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convierte un OID string a array de bytes
        /// </summary>
        /// <param name="oid">OID en formato string</param>
        /// <returns>Array de bytes del OID</returns>
        private byte[] ParseOidToBytes(string oid)
        {
            var parts = oid.Split('.').Select(int.Parse).ToArray();
            var result = new List<byte>();

            result.Add((byte)(parts[0] * 40 + parts[1]));

            for (int i = 2; i < parts.Length; i++)
            {
                var bytes = new List<byte>();
                var value = parts[i];

                do
                {
                    bytes.Insert(0, (byte)(value % 128));
                    value /= 128;
                } while (value > 0);

                for (int j = 0; j < bytes.Count; j++)
                {
                    if (j < bytes.Count - 1)
                        bytes[j] |= 0x80;
                }

                result.AddRange(bytes);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Parsea el estado del dispositivo SNMP
        /// </summary>
        /// <param name="statusValue">Valor del estado</param>
        /// <returns>Estado del dispositivo</returns>
        private string ParseDeviceStatus(string statusValue)
        {
            return statusValue switch
            {
                "1" => "Desconocido",
                "2" => "En funcionamiento",
                "3" => "Advertencia",
                "4" => "Prueba",
                "5" => "Inactivo",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Parsea el estado específico de la impresora
        /// </summary>
        /// <param name="statusValue">Valor del estado</param>
        /// <returns>Estado de la impresora</returns>
        private string ParsePrinterStatus(string statusValue)
        {
            return statusValue switch
            {
                "1" => "Otro",
                "2" => "Desconocido",
                "3" => "Inactivo",
                "4" => "En funcionamiento",
                "5" => "Fuera de servicio",
                _ => "Desconocido"
            };
        }

        /// <summary>
        /// Genera un mensaje de estado basado en los datos recolectados
        /// </summary>
        /// <param name="status">Estado de la impresora</param>
        /// <returns>Mensaje descriptivo</returns>
        private string GetStatusMessage(PrinterStatusInfo status)
        {
            if (!status.IsOnline)
                return "Impresora no responde";

            var messages = new List<string>();

            if (!string.IsNullOrEmpty(status.Status))
                messages.Add($"Estado: {status.Status}");

            return string.Join(" | ", messages);
        }
    }

    /// <summary>
    /// Estado de la impresora obtenido vía SNMP
    /// </summary>
    public class PrinterStatusInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; }
        public bool IsOnline { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
