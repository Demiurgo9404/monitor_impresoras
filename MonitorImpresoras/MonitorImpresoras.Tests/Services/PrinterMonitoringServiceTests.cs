using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Services.SNMP;
using Moq;
using Xunit;

namespace MonitorImpresoras.Tests.Services
{
    public class PrinterMonitoringServiceTests
    {
        private readonly Mock<ISnmpService> _snmpServiceMock;
        private readonly PrinterMonitoringService _service;

        public PrinterMonitoringServiceTests()
        {
            _snmpServiceMock = new Mock<ISnmpService>();
            _service = new PrinterMonitoringService(null, _snmpServiceMock.Object);
        }

        [Fact]
        public async Task MonitorPrinterAsync_WithValidPrinter_ReturnsUpdatedPrinter()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "Test Printer",
                IpAddress = "192.168.1.100",
                IsActive = true
            };

            var printerStatus = new PrinterStatus
            {
                IpAddress = "192.168.1.100",
                IsOnline = true,
                StatusMessage = "Online",
                TotalPages = 1000,
                TonerLevel = 85
            };

            _snmpServiceMock.Setup(s => s.GetPrinterStatusAsync(
                "192.168.1.100", "public", 161))
                .ReturnsAsync(printerStatus);

            // Act
            var result = await _service.MonitorPrinterAsync(printer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Printer", result.Name);
            Assert.True(result.IsOnline);
            Assert.Equal(1000, result.PageCount);
        }

        [Fact]
        public async Task MonitorPrinterAsync_WithInactivePrinter_ThrowsException()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "Test Printer",
                IpAddress = "192.168.1.100",
                IsActive = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.MonitorPrinterAsync(printer.Id));
        }

        [Fact]
        public async Task MonitorAllPrintersAsync_WithEmptyList_ReturnsZero()
        {
            // Arrange - No printers in the system

            // Act
            var result = await _service.MonitorAllPrintersAsync();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculatePrintCost_WithValidData_ReturnsCorrectCost()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Name = "Test Printer",
                CostPerPage = 0.05m
            };

            var pages = 100;

            // Act
            var cost = _service.CalculatePrintCost(pages, printer);

            // Assert
            Assert.Equal(5.0m, cost);
        }
    }
}
