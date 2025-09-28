using FluentAssertions;
using MonitorImpresoras.Application.DTOs.Printers;
using MonitorImpresoras.Application.Validators.Printers;

namespace MonitorImpresoras.Tests.Validators
{
    public class CreatePrinterDtoValidatorTests
    {
        private readonly CreatePrinterDtoValidator _validator;

        public CreatePrinterDtoValidatorTests()
        {
            _validator = new CreatePrinterDtoValidator();
        }

        [Fact]
        public void Should_HaveValidationError_When_NameIsEmpty()
        {
            // Arrange
            var dto = new CreatePrinterDto
            {
                Name = "",
                Model = "HP LaserJet",
                SerialNumber = "ABC123",
                IpAddress = "192.168.1.100"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("requerido"));
        }

        [Fact]
        public void Should_HaveValidationError_When_NameExceedsMaxLength()
        {
            // Arrange
            var dto = new CreatePrinterDto
            {
                Name = new string('A', 101), // 101 caracteres
                Model = "HP LaserJet",
                SerialNumber = "ABC123",
                IpAddress = "192.168.1.100"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("100"));
        }

        [Fact]
        public void Should_HaveValidationError_When_IpAddressIsInvalid()
        {
            // Arrange
            var dto = new CreatePrinterDto
            {
                Name = "Test Printer",
                Model = "HP LaserJet",
                SerialNumber = "ABC123",
                IpAddress = "invalid-ip"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "IpAddress" && e.ErrorMessage.Contains("IP válida"));
        }

        [Fact]
        public void Should_BeValid_When_AllFieldsAreValid()
        {
            // Arrange
            var dto = new CreatePrinterDto
            {
                Name = "Test Printer",
                Model = "HP LaserJet Pro",
                SerialNumber = "ABC123456",
                IpAddress = "192.168.1.100",
                Location = "Office Floor 1",
                Status = "Online",
                CommunityString = "public",
                SnmpPort = 161,
                Notes = "Test notes"
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("192.168.1.1")]
        [InlineData("10.0.0.1")]
        [InlineData("172.16.0.1")]
        public void Should_BeValid_When_IpAddressIsValid(string ipAddress)
        {
            // Arrange
            var dto = new CreatePrinterDto
            {
                Name = "Test Printer",
                Model = "HP LaserJet",
                SerialNumber = "ABC123",
                IpAddress = ipAddress
            };

            // Act
            var result = _validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeTrue();
        }
    }
}
