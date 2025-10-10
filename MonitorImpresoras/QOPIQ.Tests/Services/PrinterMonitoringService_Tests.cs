using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using QOPIQ.Application.Exceptions;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Services;
using QOPIQ.Tests.Builders;
using QOPIQ.Tests.Helpers;
using Xunit;

namespace QOPIQ.Tests.Services
{
    public class PrinterMonitoringServiceTests : IDisposable
    {
        private readonly Mock<IPrinterRepository> _mockPrinterRepository;
        private readonly Mock<ISnmpService> _mockSnmpService;
        private readonly Mock<ILogger<PrinterMonitoringService>> _mockLogger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PrinterMonitoringService _service;
        private bool _disposed = false;

        public PrinterMonitoringServiceTests()
        {
            _mockPrinterRepository = new Mock<IPrinterRepository>();
            _mockSnmpService = new Mock<ISnmpService>();
            _mockLogger = new Mock<ILogger<PrinterMonitoringService>>();
            
            // Configurar el mock de HttpClient
            var handlerMock = HttpClientTestHelper.CreateMockHttpMessageHandler();
            _httpClientFactory = HttpClientTestHelper.CreateMockHttpClientFactory(handlerMock);
            
            _service = new PrinterMonitoringService(
                _mockPrinterRepository.Object,
                _mockSnmpService.Object,
                _mockLogger.Object,
                _httpClientFactory);
        }

        #region GetAllPrintersAsync Tests

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnEmptyList_WhenNoPrintersExist()
        {
            // Arrange
            _mockPrinterRepository.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Printer>());

            // Act
            var result = await _service.GetAllPrintersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockPrinterRepository.Verify(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnAllPrinters_WhenPrintersExist()
        {
            // Arrange
            var testPrinters = new List<Printer>
            {
                new PrinterTestBuilder().WithId(Guid.NewGuid()).WithName("Printer 1").Build(),
                new PrinterTestBuilder().WithId(Guid.NewGuid()).WithName("Printer 2").Build()
            };

            _mockPrinterRepository.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPrinters);

            // Act
            var result = await _service.GetAllPrintersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.Name == "Printer 1");
            Assert.Contains(result, p => p.Name == "Printer 2");
        }

        #endregion

        #region GetPrinterByIdAsync Tests

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

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPrinter);

            // Act
            var result = await _service.GetPrinterByIdAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testId, result.Id);
            Assert.Equal("Test Printer", result.Name);
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldThrowNotFoundException_WhenPrinterDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetPrinterByIdAsync(testId));
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region AddPrinterAsync Tests

        [Fact]
        public async Task AddPrinterAsync_ShouldAddPrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var testPrinter = new PrinterTestBuilder()
                .WithName("New Printer")
                .WithIpAddress("192.168.1.101")
                .WithSnmpPort(161)
                .Build();

            _mockPrinterRepository.Setup(x => x.AddAsync(It.IsAny<Printer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPrinter);

            // Act
            var result = await _service.AddPrinterAsync(testPrinter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Printer", result.Name);
            _mockPrinterRepository.Verify(x => x.AddAsync(It.IsAny<Printer>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldValidatePrinter_WhenAddingPrinter()
        {
            // Arrange
            var invalidPrinter = new PrinterTestBuilder()
                .WithName("") // Nombre vacío es inválido
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddPrinterAsync(invalidPrinter));
            _mockPrinterRepository.Verify(x => x.AddAsync(It.IsAny<Printer>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdatePrinterAsync Tests

        [Fact]
        public async Task UpdatePrinterAsync_ShouldUpdatePrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithName("Old Name")
                .WithIpAddress("192.168.1.100")
                .Build();

            var updatedPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithName("Updated Name")
                .WithIpAddress("192.168.1.100")
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPrinter);
            _mockPrinterRepository.Setup(x => x.UpdateAsync(It.IsAny<Printer>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedPrinter);

            // Act
            var result = await _service.UpdatePrinterAsync(updatedPrinter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.IsAny<Printer>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region DeletePrinterAsync Tests

        [Fact]
        public async Task DeletePrinterAsync_ShouldDeletePrinter_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPrinter);
            _mockPrinterRepository.Setup(x => x.DeleteAsync(testId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeletePrinterAsync(testId);

            // Assert
            _mockPrinterRepository.Verify(x => x.DeleteAsync(testId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldThrowNotFoundException_WhenPrinterDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeletePrinterAsync(testId));
            _mockPrinterRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region TestPrinterConnectionAsync Tests

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldReturnTrue_WhenPrinterIsReachable()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            var port = 161;

            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress, port, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress, port);

            // Assert
            Assert.True(result);
            _mockSnmpService.Verify(x => x.TestConnectionAsync(ipAddress, port, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldReturnFalse_WhenPingFails()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            var port = 161;

            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress, port, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new PingException("Ping failed"));

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress, port);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetPrinterStatusAsync Tests

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnStatus_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithIpAddress("192.168.1.100")
                .WithSnmpPort(161)
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPrinter);
            _mockSnmpService.Setup(x => x.TestConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockSnmpService.Setup(x => x.GetPrinterStatusAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new Dictionary<string, object>
                {
                    ["tonerLevel"] = 85,
                    ["pageCount"] = 1000
                });

            // Act
            var status = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(status);
            Assert.Equal(testId, status["printerId"]);
            Assert.True((bool)status["isOnline"]);
            Assert.NotNull(status["timestamp"]);
            
            // Verificar que se incluyan las métricas SNMP
            Assert.Equal(85, status["tonerLevel"]);
            Assert.Equal(1000, status["pageCount"]);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnOfflineStatus_WhenPrinterIsUnreachable()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new PrinterTestBuilder()
                .WithId(testId)
                .WithIpAddress("192.168.1.100")
                .WithSnmpPort(161)
                .Build();

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPrinter);
            _mockSnmpService.Setup(x => x.TestConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var status = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(status);
            Assert.False((bool)status["isOnline"]);
            Assert.NotNull(status["timestamp"]);
        }

        #endregion

        #region IDisposable Implementation

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
                    _service?.Dispose();
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
