using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using QOPIQ.Infrastructure.Services;

namespace QOPIQ.Tests.Services
{
    public class WindowsPrinterServiceTests
    {
        private readonly Mock<ILogger<WindowsPrinterService>> _loggerMock;
        private readonly WindowsPrinterService _service;

        public WindowsPrinterServiceTests()
        {
            _loggerMock = new Mock<ILogger<WindowsPrinterService>>();
            _service = new WindowsPrinterService(_loggerMock.Object);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnStatus_WhenPrinterExists()
        {
            // Arrange
            string printerName = "TestPrinter";
            
            // Act
            var result = await _service.GetPrinterStatusAsync(printerName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("Status"));
            Assert.True(result.ContainsKey("IsOnline"));
            Assert.True(result.ContainsKey("PagesPrinted"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetPrinterStatusAsync_ShouldHandleInvalidInput(string printerName)
        {
            // Act
            var result = await _service.GetPrinterStatusAsync(printerName);

            // Assert
            Assert.NotNull(result);
            Assert.False((bool)result["IsOnline"]);
            Assert.Contains("nombre no válido", result["StatusMessage"].ToString().ToLower());
        }

        [Fact]
        public async Task IsPrinterOnlineAsync_ShouldReturnFalse_WhenPrinterDoesNotExist()
        {
            // Arrange
            string nonExistentPrinter = "NonExistentPrinter";

            // Act
            bool isOnline = await _service.IsPrinterOnlineAsync(nonExistentPrinter);

            // Assert
            Assert.False(isOnline);
        }

        [Fact]
        public async Task GetPrinterPageCountAsync_ShouldReturnZero_WhenPrinterNotAvailable()
        {
            // Arrange
            string printerName = "UnavailablePrinter";

            // Act
            int pageCount = await _service.GetPrinterPageCountAsync(printerName);

            // Assert
            Assert.Equal(0, pageCount);
        }
    }
}

