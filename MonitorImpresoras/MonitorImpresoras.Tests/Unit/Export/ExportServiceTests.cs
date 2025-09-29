using FluentAssertions;
using Moq;
using MonitorImpresoras.Application.Services.Export;

namespace MonitorImpresoras.Tests.Unit.Export
{
    public class PdfExportServiceTests
    {
        private readonly PdfExportService _pdfExportService;

        public PdfExportServiceTests()
        {
            _pdfExportService = new PdfExportService(Mock.Of<ILogger<PdfExportService>>());
        }

        [Fact]
        public async Task GeneratePdfAsync_WithValidData_ShouldReturnPdfBytes()
        {
            // Arrange
            var reportTitle = "Test Report";
            var reportDescription = "Test Description";
            var data = new List<object>
            {
                new { Id = 1, Name = "Test Item 1", Status = "Active" },
                new { Id = 2, Name = "Test Item 2", Status = "Inactive" }
            };
            var userName = "testuser";
            var parameters = new Dictionary<string, object>
            {
                { "DateFrom", "2025-01-01" },
                { "DateTo", "2025-01-31" }
            };

            // Act
            var result = await _pdfExportService.GeneratePdfAsync(reportTitle, reportDescription, data, userName, parameters);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GeneratePdfAsync_WithEmptyData_ShouldReturnPdfBytes()
        {
            // Arrange
            var reportTitle = "Empty Report";
            var reportDescription = "Report with no data";
            var data = new List<object>();
            var userName = "testuser";

            // Act
            var result = await _pdfExportService.GeneratePdfAsync(reportTitle, reportDescription, data, userName);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GeneratePrinterPdfAsync_WithPrinterData_ShouldReturnPdfBytes()
        {
            // Arrange
            var printerData = new List<object>
            {
                new { Id = 1, Name = "Printer 1", Brand = "HP", Status = "Active", TonerLevel = "85%" },
                new { Id = 2, Name = "Printer 2", Brand = "Canon", Status = "Maintenance", TonerLevel = "45%" }
            };
            var userName = "admin";

            // Act
            var result = await _pdfExportService.GeneratePrinterPdfAsync(printerData, userName);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateFileName_WithValidTitle_ShouldReturnValidFileName()
        {
            // Arrange
            var reportTitle = "Test Report with Spaces & Special Characters!";

            // Act
            var result = _pdfExportService.GenerateFileName(reportTitle);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Test_Report_with_Spaces_Special_Characters");
            result.Should().EndWith(".pdf");
            result.Should().NotContain("!");
            result.Should().NotContain("&");
        }
    }

    public class ExcelExportServiceTests
    {
        private readonly ExcelExportService _excelExportService;

        public ExcelExportServiceTests()
        {
            _excelExportService = new ExcelExportService(Mock.Of<ILogger<ExcelExportService>>());
        }

        [Fact]
        public async Task GenerateExcelAsync_WithValidData_ShouldReturnExcelBytes()
        {
            // Arrange
            var reportTitle = "Test Report";
            var reportDescription = "Test Description";
            var data = new List<object>
            {
                new { Id = 1, Name = "Test Item 1", Status = "Active", Value = 100.50 },
                new { Id = 2, Name = "Test Item 2", Status = "Inactive", Value = 250.75 }
            };
            var userName = "testuser";
            var parameters = new Dictionary<string, object>
            {
                { "Category", "Test" },
                { "Limit", 100 }
            };

            // Act
            var result = await _excelExportService.GenerateExcelAsync(reportTitle, reportDescription, data, userName, parameters);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GenerateExcelAsync_WithEmptyData_ShouldReturnExcelBytes()
        {
            // Arrange
            var reportTitle = "Empty Report";
            var reportDescription = "Report with no data";
            var data = new List<object>();
            var userName = "testuser";

            // Act
            var result = await _excelExportService.GenerateExcelAsync(reportTitle, reportDescription, data, userName);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GeneratePrinterExcelAsync_WithPrinterData_ShouldReturnExcelBytes()
        {
            // Arrange
            var printerData = new List<object>
            {
                new { Id = 1, Name = "Printer 1", Brand = "HP", Model = "LaserJet", Status = "Active", TonerLevel = "85%" },
                new { Id = 2, Name = "Printer 2", Brand = "Canon", Model = "PIXMA", Status = "Maintenance", TonerLevel = "45%" }
            };
            var userName = "admin";

            // Act
            var result = await _excelExportService.GeneratePrinterExcelAsync(printerData, userName);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GenerateUserExcelAsync_WithUserData_ShouldReturnExcelBytes()
        {
            // Arrange
            var userData = new List<object>
            {
                new { Id = "1", UserName = "user1", Email = "user1@test.com", Department = "IT", IsActive = true },
                new { Id = "2", UserName = "user2", Email = "user2@test.com", Department = "HR", IsActive = false }
            };
            var userName = "admin";

            // Act
            var result = await _excelExportService.GenerateUserExcelAsync(userData, userName);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GenerateFileName_WithValidTitle_ShouldReturnValidFileName()
        {
            // Arrange
            var reportTitle = "Test Report with Spaces & Special Characters!";

            // Act
            var result = _excelExportService.GenerateFileName(reportTitle);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Test_Report_with_Spaces_Special_Characters");
            result.Should().EndWith(".xlsx");
            result.Should().NotContain("!");
            result.Should().NotContain("&");
        }
    }
}
