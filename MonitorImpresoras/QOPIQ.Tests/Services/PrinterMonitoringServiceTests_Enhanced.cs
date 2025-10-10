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
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace QOPIQ.Tests.Services
{
    public class PrinterMonitoringServiceEnhancedTests : IDisposable
    {
        private readonly Mock<IPrinterRepository> _mockPrinterRepository;
        private readonly Mock<ISnmpService> _mockSnmpService;
        private readonly Mock<ILogger<PrinterMonitoringService>> _mockLogger;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly PrinterMonitoringService _service;
        private bool _disposed = false;

        public PrinterMonitoringServiceEnhancedTests()
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
                    _service?.Dispose();
                }
                _disposed = true;
            }
        }

        #region GetAllPrintersAsync Tests

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
        public async Task GetAllPrintersAsync_ShouldReturnActivePrinters_WhenIncludeInactiveIsFalse()
        {
            // Arrange
            var testPrinters = new List<Printer>
            {
                new Printer { Id = Guid.NewGuid(), Name = "Active Printer", IsActive = true },
                new Printer { Id = Guid.NewGuid(), Name = "Inactive Printer", IsActive = false }
            };

            _mockPrinterRepository.Setup(x => x.GetAllAsync(false))
                .ReturnsAsync(testPrinters.Where(p => p.IsActive).ToList());

            // Act
            var result = await _service.GetAllPrintersAsync(includeInactive: false);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, p => Assert.True(p.IsActive));
            _mockPrinterRepository.Verify(x => x.GetAllAsync(false), Times.Once);
        }

        [Fact]
        public async Task GetAllPrintersAsync_ShouldReturnAllPrinters_WhenIncludeInactiveIsTrue()
        {
            // Arrange
            var testPrinters = new List<Printer>
            {
                new Printer { Id = Guid.NewGuid(), Name = "Active Printer", IsActive = true },
                new Printer { Id = Guid.NewGuid(), Name = "Inactive Printer", IsActive = false }
            };

            _mockPrinterRepository.Setup(x => x.GetAllAsync(true))
                .ReturnsAsync(testPrinters);

            // Act
            var result = await _service.GetAllPrintersAsync(includeInactive: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _mockPrinterRepository.Verify(x => x.GetAllAsync(true), Times.Once);
        }

        [Fact]
        public async Task GetAllPrintersAsync_ShouldHandleRepositoryException()
        {
            // Arrange
            _mockPrinterRepository.Setup(x => x.GetAllAsync(It.IsAny<bool>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<ServiceException>(() => _service.GetAllPrintersAsync());
        }

        #endregion

        #region GetPrinterByIdAsync Tests

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldReturnPrinter_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new Printer { Id = testId, Name = "Test Printer", IpAddress = "192.168.1.100" };

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
        public async Task GetPrinterByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPrinterByIdAsync(Guid.Empty));
            _mockPrinterRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region AddPrinterAsync Tests

        [Fact]
        public async Task AddPrinterAsync_ShouldAddPrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var testPrinter = new Printer 
            { 
                Name = "New Printer", 
                IpAddress = "192.168.1.100",
                Model = "Test Model"
            };

            _mockPrinterRepository.Setup(x => x.AddAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => 
                {
                    p.Id = Guid.NewGuid();
                    p.CreatedAt = DateTime.UtcNow;
                    p.UpdatedAt = DateTime.UtcNow;
                    return p;
                });

            _mockSnmpService.Setup(x => x.TestConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddPrinterAsync(testPrinter);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.True(result.IsActive);
            Assert.NotNull(result.CreatedAt);
            Assert.NotNull(result.UpdatedAt);
            _mockPrinterRepository.Verify(x => x.AddAsync(It.IsAny<Printer>()), Times.Once);
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldThrowArgumentNullException_WhenPrinterIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddPrinterAsync(null));
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldThrowValidationException_WhenPrinterNameIsEmpty()
        {
            // Arrange
            var testPrinter = new Printer { Name = string.Empty, IpAddress = "192.168.1.100" };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.AddPrinterAsync(testPrinter));
        }

        [Fact]
        public async Task AddPrinterAsync_ShouldLogWarning_WhenPrinterIsNotReachable()
        {
            // Arrange
            var testPrinter = new Printer 
            { 
                Name = "Unreachable Printer", 
                IpAddress = "192.168.1.200" 
            };

            _mockSnmpService.Setup(x => x.TestConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockPrinterRepository.Setup(x => x.AddAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => p);

            // Act
            var result = await _service.AddPrinterAsync(testPrinter);

            // Assert
            Assert.NotNull(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("is not reachable")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region TestPrinterConnectionAsync Tests

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldReturnTrue_WhenPrinterIsReachable()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldReturnFalse_WhenPrinterIsNotReachable()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldThrowArgumentException_WhenIpAddressIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.TestPrinterConnectionAsync(string.Empty));
        }

        [Fact]
        public async Task TestPrinterConnectionAsync_ShouldUseCustomPort_WhenProvided()
        {
            // Arrange
            var ipAddress = "192.168.1.100";
            var port = 1610;
            
            _mockSnmpService.Setup(x => x.TestConnectionAsync(ipAddress, port, It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.TestPrinterConnectionAsync(ipAddress, port);

            // Assert
            Assert.True(result);
            _mockSnmpService.Verify(x => x.TestConnectionAsync(ipAddress, port, It.IsAny<int>()), Times.Once);
        }

        #endregion

        #region GetPrinterStatusAsync Tests

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnStatus_WhenPrinterIsOnline()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Test Printer", 
                IpAddress = "192.168.1.100",
                SnmpPort = 161
            };

            var testStatus = new Dictionary<string, object>
            {
                { "status", "Ready" },
                { "tonerLevel", 80 },
                { "pageCount", 1000 }
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            _mockSnmpService.Setup(x => x.TestConnectionAsync(testPrinter.IpAddress, testPrinter.SnmpPort.Value, It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockSnmpService.Setup(x => x.GetPrinterStatusAsync(testPrinter.IpAddress, testPrinter.SnmpPort.Value))
                .ReturnsAsync(testStatus);

            // Act
            var result = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsOnline);
            Assert.Equal(testId, result.PrinterId);
            Assert.NotNull(result.Timestamp);
            Assert.NotNull(result.Details);
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.IsAny<Printer>()), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldReturnOfflineStatus_WhenPrinterIsNotReachable()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Offline Printer", 
                IpAddress = "192.168.1.101",
                SnmpPort = 161
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            _mockSnmpService.Setup(x => x.TestConnectionAsync(testPrinter.IpAddress, testPrinter.SnmpPort.Value, It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsOnline);
            Assert.Equal(testId, result.PrinterId);
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.IsAny<Printer>()), Times.Once);
        }

        [Fact]
        public async Task GetPrinterStatusAsync_ShouldHandleSnmpErrors_Gracefully()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Test Printer", 
                IpAddress = "192.168.1.102",
                SnmpPort = 161
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            _mockSnmpService.Setup(x => x.TestConnectionAsync(testPrinter.IpAddress, testPrinter.SnmpPort.Value, It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockSnmpService.Setup(x => x.GetPrinterStatusAsync(testPrinter.IpAddress, testPrinter.SnmpPort.Value))
                .ThrowsAsync(new Exception("SNMP timeout"));

            // Act
            var result = await _service.GetPrinterStatusAsync(testId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsOnline);
            Assert.NotNull(result.Error);
            Assert.Contains("SNMP", result.Error);
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
        public async Task GetPrinterStatusAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPrinterStatusAsync(Guid.Empty));
        }

        #endregion

        #region UpdatePrinterAsync Tests

        [Fact]
        public async Task UpdatePrinterAsync_ShouldUpdatePrinter_WhenValidPrinterProvided()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Old Name", 
                IpAddress = "192.168.1.100",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedPrinter = new Printer 
            { 
                Id = testId, 
                Name = "New Name", 
                IpAddress = "192.168.1.100",
                Model = "Updated Model"
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(existingPrinter);

            _mockPrinterRepository.Setup(x => x.UpdateAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => p);

            // Act
            await _service.UpdatePrinterAsync(updatedPrinter);

            // Assert
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.Is<Printer>(p => 
                p.Name == "New Name" && 
                p.Model == "Updated Model" &&
                p.UpdatedAt > p.CreatedAt)), 
                Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldThrowNotFoundException_WhenPrinterDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync((Printer)null);

            var printerToUpdate = new Printer { Id = testId, Name = "Non-existent" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdatePrinterAsync(printerToUpdate));
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldUpdateLastUpdatedTimestamp()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Old Name", 
                IpAddress = "192.168.1.100",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Updated Name", 
                IpAddress = "192.168.1.100"
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(existingPrinter);

            _mockPrinterRepository.Setup(x => x.UpdateAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => p);

            // Act
            await _service.UpdatePrinterAsync(updatedPrinter);

            // Assert
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.Is<Printer>(p => 
                p.UpdatedAt > p.CreatedAt)), 
                Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldValidatePrinterBeforeUpdating()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var existingPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Valid Name", 
                IpAddress = "192.168.1.100"
            };

            var invalidPrinter = new Printer 
            { 
                Id = testId, 
                Name = "", // Invalid: empty name
                IpAddress = "192.168.1.100"
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(existingPrinter);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.UpdatePrinterAsync(invalidPrinter));
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.IsAny<Printer>()), Times.Never);
        }

        #endregion

        #region DeletePrinterAsync Tests

        [Fact]
        public async Task DeletePrinterAsync_ShouldMarkPrinterAsInactive_WhenPrinterExists()
        {
            // Arrange
            var testId = Guid.NewGuid();
            var testPrinter = new Printer 
            { 
                Id = testId, 
                Name = "Test Printer", 
                IsActive = true 
            };

            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync(testPrinter);

            _mockPrinterRepository.Setup(x => x.UpdateAsync(It.IsAny<Printer>()))
                .ReturnsAsync((Printer p) => p);

            // Act
            await _service.DeletePrinterAsync(testId);

            // Assert
            _mockPrinterRepository.Verify(x => x.UpdateAsync(It.Is<Printer>(p => 
                !p.IsActive && 
                p.UpdatedAt != null)), 
                Times.Once);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldThrowNotFoundException_WhenPrinterDoesNotExist()
        {
            // Arrange
            var testId = Guid.NewGuid();
            _mockPrinterRepository.Setup(x => x.GetByIdAsync(testId))
                .ReturnsAsync((Printer)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeletePrinterAsync(testId));
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeletePrinterAsync(Guid.Empty));
        }

        #endregion
    }
}
