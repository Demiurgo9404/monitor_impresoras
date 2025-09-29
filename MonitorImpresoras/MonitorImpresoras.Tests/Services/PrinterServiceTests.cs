using FluentAssertions;
using Moq;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Services
{
    public class PrinterServiceTests
    {
        private readonly Mock<IPrinterRepository> _mockRepository;
        private readonly PrinterService _service;

        public PrinterServiceTests()
        {
            _mockRepository = new Mock<IPrinterRepository>();
            _service = new PrinterService(_mockRepository.Object);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldReturnPrinter_WhenPrinterExists()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            var expectedPrinter = new Printer
            {
                Id = printerId,
                Name = "Test Printer",
                Model = "HP LaserJet",
                IpAddress = "192.168.1.100"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(printerId))
                .ReturnsAsync(expectedPrinter);

            // Act
            var result = await _service.GetPrinterByIdAsync(printerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedPrinter);
            _mockRepository.Verify(r => r.GetByIdAsync(printerId), Times.Once);
        }

        [Fact]
        public async Task GetPrinterByIdAsync_ShouldReturnNull_WhenPrinterDoesNotExist()
        {
            // Arrange
            var printerId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(printerId))
                .ReturnsAsync((Printer?)null);

            // Act
            var result = await _service.GetPrinterByIdAsync(printerId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByIdAsync(printerId), Times.Once);
        }

        [Fact]
        public async Task CreatePrinterAsync_ShouldCallRepository_AndReturnCreatedPrinter()
        {
            // Arrange
            var newPrinter = new Printer
            {
                Name = "New Printer",
                Model = "Epson WorkForce",
                SerialNumber = "XYZ789",
                IpAddress = "192.168.1.200"
            };

            _mockRepository.Setup(r => r.AddAsync(newPrinter))
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreatePrinterAsync(newPrinter);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(newPrinter);
            _mockRepository.Verify(r => r.AddAsync(newPrinter), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldReturnUpdatedPrinter_WhenPrinterExists()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            var existingPrinter = new Printer
            {
                Id = printerId,
                Name = "Original Printer",
                Model = "HP Original",
                IpAddress = "192.168.1.100"
            };

            var updatedPrinter = new Printer
            {
                Name = "Updated Printer",
                Model = "HP Updated",
                IpAddress = "192.168.1.101"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(printerId))
                .ReturnsAsync(existingPrinter);
            _mockRepository.Setup(r => r.UpdateAsync(existingPrinter))
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdatePrinterAsync(printerId, updatedPrinter);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Printer");
            result.Model.Should().Be("HP Updated");
            result.IpAddress.Should().Be("192.168.1.101");
            _mockRepository.Verify(r => r.GetByIdAsync(printerId), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(existingPrinter), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdatePrinterAsync_ShouldReturnNull_WhenPrinterDoesNotExist()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            var updatedPrinter = new Printer
            {
                Name = "Updated Printer",
                Model = "HP Updated"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(printerId))
                .ReturnsAsync((Printer?)null);

            // Act
            var result = await _service.UpdatePrinterAsync(printerId, updatedPrinter);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByIdAsync(printerId), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Printer>()), Times.Never);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldReturnTrue_WhenPrinterExists()
        {
            // Arrange
            var printerId = Guid.NewGuid();
            var existingPrinter = new Printer
            {
                Id = printerId,
                Name = "Printer to Delete"
            };

            _mockRepository.Setup(r => r.GetByIdAsync(printerId))
                .ReturnsAsync(existingPrinter);
            _mockRepository.Setup(r => r.DeleteAsync(printerId))
                .Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeletePrinterAsync(printerId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(r => r.GetByIdAsync(printerId), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(printerId), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePrinterAsync_ShouldReturnFalse_WhenPrinterDoesNotExist()
        {
            // Arrange
            var printerId = Guid.NewGuid();

            _mockRepository.Setup(r => r.GetByIdAsync(printerId))
                .ReturnsAsync((Printer?)null);

            // Act
            var result = await _service.DeletePrinterAsync(printerId);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(r => r.GetByIdAsync(printerId), Times.Once);
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
