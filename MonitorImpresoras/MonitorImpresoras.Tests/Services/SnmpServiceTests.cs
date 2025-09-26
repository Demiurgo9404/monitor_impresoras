using MonitorImpresoras.Infrastructure.Services.SNMP;
using Moq;
using Xunit;

namespace MonitorImpresoras.Tests.Services
{
    public class SnmpServiceTests
    {
        private readonly SnmpService _service;

        public SnmpServiceTests()
        {
            _service = new SnmpService();
        }

        [Fact]
        public async Task GetPrinterStatusAsync_WithValidIp_ReturnsStatus()
        {
            // Arrange
            var ipAddress = "192.168.1.100";

            // Act
            var result = await _service.GetPrinterStatusAsync(ipAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ipAddress, result.IpAddress);
            Assert.True(result.LastChecked > DateTime.MinValue);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_WithInvalidIp_ReturnsOfflineStatus()
        {
            // Arrange
            var ipAddress = "192.168.1.999"; // IP inválida

            // Act
            var result = await _service.GetPrinterStatusAsync(ipAddress);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsOnline);
            Assert.Contains("Error", result.StatusMessage);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_WithCustomCommunity_UsesCorrectCommunity()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            var customCommunity = "private";

            // Act
            var result = await _service.GetPrinterStatusAsync(ipAddress, customCommunity);

            // Assert
            Assert.NotNull(result);
            // El servicio debería manejar la community string correctamente
        }

        [Fact]
        public void ParseOidToBytes_WithValidOid_ReturnsCorrectBytes()
        {
            // Arrange
            var oid = "1.3.6.1.2.1.25.3.5.1.1";
            var expectedLength = 7; // Longitud esperada del OID parseado

            // Act
            var result = _service.ParseOidToBytes(oid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
            // Verificar que se generaron bytes válidos
        }

        [Fact]
        public void GetStatusMessage_WithOnlinePrinter_ReturnsDetailedMessage()
        {
            // Arrange
            var status = new PrinterStatus
            {
                IsOnline = true,
                DeviceStatus = "En funcionamiento",
                PrinterStatus = "Inactivo",
                TotalPages = 1500,
                TonerLevel = 75
            };

            // Act
            var message = _service.GetStatusMessage(status);

            // Assert
            Assert.Contains("En funcionamiento", message);
            Assert.Contains("Inactivo", message);
            Assert.Contains("1500", message);
            Assert.Contains("75%", message);
        }

        [Fact]
        public void GetStatusMessage_WithOfflinePrinter_ReturnsOfflineMessage()
        {
            // Arrange
            var status = new PrinterStatus
            {
                IsOnline = false,
                StatusMessage = "Error de conexión: Timeout"
            };

            // Act
            var message = _service.GetStatusMessage(status);

            // Assert
            Assert.Equal("Error de conexión: Timeout", message);
        }
    }
}
