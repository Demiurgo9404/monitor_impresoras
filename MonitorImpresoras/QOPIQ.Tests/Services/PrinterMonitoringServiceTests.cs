using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using QOPIQ.Application.Exceptions;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Infrastructure.Services;
using Xunit;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Http;
using System.Net.Http;
using Moq.Protected;
using System.Threading;
using System.Net.Http.Headers;
using System.Text;

namespace QOPIQ.Tests.Services
{
    public class PrinterMonitoringServiceTests : IDisposable
    {
        private readonly Mock<IPrinterRepository> _mockPrinterRepository;
        private readonly Mock<ISnmpService> _mockSnmpService;
        private readonly Mock<ILogger<PrinterMonitoringService>> _mockLogger;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly PrinterMonitoringService _service;
        private bool _disposed = false;

        public PrinterMonitoringServiceTests()
        {
            _mockPrinterRepository = new Mock<IPrinterRepository>();
            _mockSnmpService = new Mock<ISnmpService>();
            _mockLogger = new Mock<ILogger<PrinterMonitoringService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            
            // Setup default HTTP client factory mock
            var handler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            
            _service = new PrinterMonitoringService(
                _mockPrinterRepository.Object,
                _mockSnmpService.Object,
                _mockLogger.Object,
                _mockHttpClientFactory.Object);
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

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnPrinters_WhenPrintersExist()
        {
            // Arrange
            var testPrinters = new List<Printer>
            {
                new Printer { Id = Guid.NewGuid(), Name = "Printer 1", IpAddress = "192.168.1.1", IsActive = true },
                new Printer { Id = Guid.NewGuid(), Name = "Printer 2", IpAddress = "192.168.1.2", IsActive = true }
            };
            
            _mockPrinterRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(testPrinters);

            // Act
            var result = await _service.GetAllPrintersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(testPrinters[0].Name, result[0].Name);
            Assert.Equal(testPrinters[1].Name, result[1].Name);
        }

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnEmptyList_WhenNoPrintersExist()
        {
            // Arrange
            _mockPrinterRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Printer>());

            // Act
            var result = await _service.GetAllPrintersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldReturnPrinter_WhenPrinterExists()
        {
            // Arrange
            var testPrinter = new Printer { Id = Guid.NewGuid(), Name = "Test Printer", IpAddress = "192.168.1.10" };
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testPrinter.Id))
                .ReturnsAsync(testPrinter);

            // Act
            var result = await _service.GetPrinterByIdAsync(testPrinter.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testPrinter.Name, result.Name);
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(testPrinter.Id), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldThrowException_WhenPrinterNotFound()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(printerId))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetPrinterStatusAsync(printerId));
            
            Assert.Contains(printerId.ToString(), exception.Message);
            _mockLogger.Verify(
                x => x.LogWarning("No se encontró la impresora con ID: {PrinterId}", printerId),
                Times.Once);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldThrowException_WhenIdIsEmpty()
        {
{{ ... }}
            // Arrange
            var emptyId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPrinterByIdAsync(emptyId));
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldAddPrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var newPrinter = new Printer { Name = "New Printer", IpAddress = "192.168.1.100" };
            var addedPrinter = new Printer { 
                Id = Guid.NewGuid(), 
                Name = newPrinter.Name, 
                IpAddress = newPrinter.IpAddress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _mockPrinterRepository.Setup(x => x.AddAsync(It.IsAny<Printer>()))
                .ReturnsAsync(addedPrinter);

            // Act
            var result = await _service.AddPrinterAsync(newPrinter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedPrinter.Id, result.Id);
            Assert.Equal(newPrinter.Name, result.Name);
            Assert.NotEqual(default(DateTime), result.CreatedAt);
            Assert.NotEqual(default(DateTime), result.UpdatedAt);
            _mockPrinterRepository.Verify(x => x.AddAsync(It.Is<Printer>(p => 
                p.Name == newPrinter.Name && 
                p.IpAddress == newPrinter.IpAddress &&
                p.CreatedAt != default(DateTime) &&
                p.UpdatedAt != default(DateTime))), 
                Times.Once);
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Agregando nueva impresora con nombre: {PrinterName}", 
                    newPrinter.Name),
                Times.Once);
            _mockLogger.Verify(
                x => x.LogInformation(
                    "Impresora agregada exitosamente con ID: {PrinterId}", 
                    addedPrinter.Id),
                Times.Once);
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldThrowValidationException_WhenNameIsEmpty()
        {
            // Arrange
            var newPrinter = new Printer { Name = string.Empty, IpAddress = "192.168.1.100" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.AddPrinterAsync(newPrinter));
            Assert.Contains("El nombre de la impresora es requerido", exception.Message);
            _mockPrinterRepository.Verify(x => x.AddAsync(It.IsAny<Printer>()), Times.Never);
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldThrowValidationException_WhenIpAddressIsInvalid()
        {
            // Arrange
            var newPrinter = new Printer { Name = "Test Printer", IpAddress = "invalid-ip-address" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.AddPrinterAsync(newPrinter));
            Assert.Contains("La dirección IP no es válida", exception.Message);
            _mockPrinterRepository.Verify(x => x.AddAsync(It.IsAny<Printer>()), Times.Never);
        }

        [Theory]
        [InlineData("192.168.1.100", true)]
        [InlineData("192.168.1.101", false)]
        [InlineData("invalid-ip", false)]
        public async Task TestPrinterConnectionAsync_ShouldReturnExpectedResult(string ipAddress, bool expectedResult)
        {
            // Arrange
            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockSnmpService.Verify(x => x.TestConnectionAsync(ipAddress), Times.Once);
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldHandleNullIpAddress()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.TestPrinterConnectionAsync(null));
            Assert.Equal("ipAddress", exception.ParamName);
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldReturnFalse_WhenPingFails()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress))
                .ThrowsAsync(new Exception("Connection failed"));

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.LogWarning(It.IsAny<Exception>(), "Error al probar la conexión con la impresora {IpAddress}", ipAddress),
                Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnStatus_WhenPrinterExists()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            var testPrinter = new Printer { Id = printerId, IpAddress = "192.168.1.100" };
            var expectedStatus = new Dictionary<string, object>
            {
                { "Status", "Online" },
                { "TonerLevel", 85 },
                { "PaperStatus", "Ready" }
            };
            
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(printerId))
                .ReturnsAsync(testPrinter);
            _mockSnmpService.Setup(x => x.GetPrinterStatusAsync(testPrinter.IpAddress))
                .ReturnsAsync(expectedStatus);

            // Act
            var result = await _service.GetPrinterStatusAsync(printerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Online", result["Status"]);
            Assert.Equal(85, result["TonerLevel"]);
            Assert.Equal("Ready", result["PaperStatus"]);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldThrowException_WhenPrinterNotFound()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(printerId))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetPrinterStatusAsync(printerId));
            
            Assert.Contains(printerId.ToString(), exception.Message);
            _mockLogger.Verify(
                x => x.LogWarning("No se encontró la impresora con ID: {PrinterId}", printerId),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldUpdatePrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var printer = new Printer { Id = Guid.NewGuid(), Name = "Updated Printer", IpAddress = "192.168.1.100" };
            
            // Act
            await _service.UpdatePrinterAsync(printer);

            // Assert
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.Is<Printer>(p => 
                p.Id == printer.Id && 
                p.Name == printer.Name && 
                p.IpAddress == printer.IpAddress)), 
                Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldThrowException_WhenPrinterIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.UpdatePrinterAsync(null));
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.IsAny<Printer>()), Times.Never);
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<ArgumentNullException>(),
                    "Attempted to update a null printer"),
                Times.Once);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldDeletePrinter_WhenValidIdProvided()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            
            // Act
            await _service.DeletePrinterAsync(printerId);

            // Assert
            _mockPrinterRepository.Verify(x => x.DeleteAsync(printerId), Times.Once);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldThrowException_WhenIdIsEmpty()
        {
            // Arrange
            var emptyId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.DeletePrinterAsync(emptyId));
            _mockPrinterRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Attempted to delete printer with empty ID"),
                Times.Once);
        }

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnEmptyList_WhenNoPrintersExist()
        {
            // Arrange
            _mockPrinterRepository.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<Printer>());

            // Act
            var result = await _service.GetAllPrintersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockPrinterRepository.Verify(x => x.GetAllAsync(), Times.Once);
        }
    }
}
