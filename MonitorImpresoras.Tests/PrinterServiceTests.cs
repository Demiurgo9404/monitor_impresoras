using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MonitorImpresoras.Tests.Services
{
    public class PrinterServiceTests
    {
        private readonly Mock<IPrinterRepository> _mockPrinterRepository;
        private readonly Mock<ILogger<PrinterService>> _mockLogger;
        private readonly PrinterService _printerService;

        public PrinterServiceTests()
        {
            _mockPrinterRepository = new Mock<IPrinterRepository>();
            _mockLogger = new Mock<ILogger<PrinterService>>();
            _printerService = new PrinterService(_mockPrinterRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetPrintersAsync_ShouldReturnPrinterNames_WhenPrintersExist()
        {
            // Arrange
            var mockPrinters = new List<Printer>
            {
                new Printer { Id = 1, Name = "HP LaserJet Pro", IsOnline = true },
                new Printer { Id = 2, Name = "Epson WorkForce", IsOnline = false },
                new Printer { Id = 3, Name = "Canon PIXMA", IsOnline = true }
            };

            _mockPrinterRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(mockPrinters);

            // Act
            var result = await _printerService.GetPrintersAsync();

            // Assert
            var printerNames = result.ToList();
            Assert.Equal(3, printerNames.Count);
            Assert.Contains("HP LaserJet Pro", printerNames);
            Assert.Contains("Epson WorkForce", printerNames);
            Assert.Contains("Canon PIXMA", printerNames);

            // Verify logging was called
            _mockLogger.Verify(logger => logger.LogInformation("Retrieved {Count} printers", 3), Times.Once);
        }

        [Fact]
        public async Task GetPrintersAsync_ShouldReturnEmptyList_WhenNoPrintersExist()
        {
            // Arrange
            _mockPrinterRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Printer>());

            // Act
            var result = await _printerService.GetPrintersAsync();

            // Assert
            Assert.Empty(result);
            _mockLogger.Verify(logger => logger.LogInformation("Retrieved {Count} printers", 0), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnOnline_WhenPrinterIsOnline()
        {
            // Arrange
            var printerId = Guid.Parse("12345678-1234-1234-1234-123456789abc");
            var mockPrinter = new Printer
            {
                Id = 1,
                Name = "HP LaserJet Pro",
                IsOnline = true,
                LastSeen = DateTime.UtcNow
            };

            _mockPrinterRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(mockPrinter);

            // Act
            var result = await _printerService.GetPrinterStatusAsync(printerId);

            // Assert
            Assert.Equal("Online", result);
            _mockLogger.Verify(logger => logger.LogInformation("Printer {PrinterName} status: {Status}", "HP LaserJet Pro", "Online"), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnOffline_WhenPrinterIsOffline()
        {
            // Arrange
            var printerId = Guid.Parse("87654321-4321-4321-4321-cba987654321");
            var mockPrinter = new Printer
            {
                Id = 2,
                Name = "Epson WorkForce",
                IsOnline = false,
                LastSeen = DateTime.UtcNow.AddHours(-2)
            };

            _mockPrinterRepository.Setup(repo => repo.GetByIdAsync(2))
                .ReturnsAsync(mockPrinter);

            // Act
            var result = await _printerService.GetPrinterStatusAsync(printerId);

            // Assert
            Assert.Equal("Offline", result);
            _mockLogger.Verify(logger => logger.LogInformation("Printer {PrinterName} status: {Status}", "Epson WorkForce", "Offline"), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldThrowKeyNotFoundException_WhenPrinterNotFound()
        {
            // Arrange
            var printerId = Guid.Parse("11111111-2222-3333-4444-555555555555");
            _mockPrinterRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync((Printer?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _printerService.GetPrinterStatusAsync(printerId));

            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldThrowArgumentException_WhenInvalidGuid()
        {
            // Arrange
            var invalidPrinterId = Guid.Parse("99999999-9999-9999-9999-999999999999");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _printerService.GetPrinterStatusAsync(invalidPrinterId));

            Assert.Contains("Invalid printer ID format", exception.Message);
        }
    }
}
