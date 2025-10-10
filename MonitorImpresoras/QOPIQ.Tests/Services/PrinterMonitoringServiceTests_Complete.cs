using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using QOPIQ.Application.Exceptions;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Services;
using Xunit;
using QOPIQ.Tests.Builders;
using QOPIQ.Tests.Helpers;
using System.Linq;

namespace QOPIQ.Tests.Services
{
    public class PrinterMonitoringServiceCompleteTests : IDisposable
    {
        private readonly Mock<IPrinterRepository> _mockPrinterRepository;
        private readonly Mock<ISnmpService> _mockSnmpService;
        private readonly Mock<ILogger<PrinterMonitoringService>> _mockLogger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PrinterMonitoringService _service;
        private bool _disposed = false;

        public PrinterMonitoringServiceCompleteTests()
        {
            _mockPrinterRepository = new Mock<IPrinterRepository>();
            _mockSnmpService = new Mock<ISnmpService>();
            _mockLogger = new Mock<ILogger<PrinterMonitoringService>>();
            
            // Setup default HTTP client
            var handlerMock = HttpClientTestHelper.CreateMockHttpMessageHandler();
            _httpClientFactory = HttpClientTestHelper.CreateMockHttpClientFactory(handlerMock);
            
            _service = new PrinterMonitoringService(
                _mockPrinterRepository.Object,
                _mockSnmpService.Object,
                _mockLogger.Object,
                _httpClientFactory);
        }

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnEmptyList_WhenNoPrintersExist()
        {
            // Arrange
            _mockPrinterRepository.Setup(x => x.GetAllAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<Printer>());

            // Act
            var result = await _service.GetAllPrintersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockPrinterRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldReturnPrinter_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithName("Test Printer")
                .WithIpAddress("192.168.1.100")
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            // Act
            var result = await _service.GetPrinterByIdAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testId, result.Id);
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(testId), Times.Once);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldThrowNotFoundException_WhenPrinterDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPrinterByIdAsync(testId));
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(testId), Times.Once);
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldAddPrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var testPrinter = new PrinterTestBuilder()
                .WithName("New Printer")
                .WithIpAddress("192.168.1.100")
                .Build();

            _mockPrinterRepository.Setup(x => x.AddAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => p);

            // Act
            var result = await _service.AddPrinterAsync(testPrinter);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            _mockPrinterRepository.Verify(x => x.AddAsync(It.IsAny<Printer>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldUpdatePrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithName("Old Name")
                .Build();

            var updatedPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithName("Updated Name")
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(existingPrinter);

            _mockPrinterRepository.Setup(x => x.UpdateAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => p);

            // Act
            await _service.UpdatePrinterAsync(updatedPrinter);

            // Assert
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.Is<Printer>(p => 
                p.Name == "Updated Name")), 
                Times.Once);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldMarkPrinterAsInactive_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            // Act
            await _service.DeletePrinterAsync(testId);

            // Assert
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.Is<Printer>(p => !p.IsActive)), 
                Times.Once);
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldReturnTrue_WhenPrinterIsReachable()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress, It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress);

            // Assert
            Assert.True(result);
            _mockSnmpService.Verify(x => x.TestConnectionAsync(ipAddress, It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnStatus_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithIpAddress("192.168.1.100")
                .Build();

            var statusData = new Dictionary<string, object>
            {
                { "status", "Online" },
                { "tonerLevel", 75 }
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            _mockSnmpService.Setup(x => x.GetPrinterStatusAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(statusData);

            // Act
            var result = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Online", result["status"]);
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(testId), Times.Once);
            _mockSnmpService.Verify(x => x.GetPrinterStatusAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldThrowNotFoundException_WhenPrinterDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPrinterStatusAsync(testId));
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldHandleSnmpErrors_Gracefully()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithIpAddress("192.168.1.100")
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            _mockSnmpService.Setup(x => x.GetPrinterStatusAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("SNMP Timeout"));

            // Act
            var result = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("error"));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _service?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
