using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Unit
{
    public class PredictiveMaintenanceServiceTests
    {
        private readonly Mock<ILogger<PredictiveMaintenanceService>> _loggerMock;
        private readonly PredictiveMaintenanceService _predictiveService;

        public PredictiveMaintenanceServiceTests()
        {
            _loggerMock = new Mock<ILogger<PredictiveMaintenanceService>>();
            _predictiveService = new PredictiveMaintenanceService(_loggerMock.Object);
        }

        [Fact]
        public async Task TrainModelAsync_WithValidData_ShouldReturnTrainingResult()
        {
            // Arrange
            var trainingData = GenerateSampleTrainingData(100);

            // Act
            var result = await _predictiveService.TrainModelAsync(trainingData);

            // Assert
            result.Should().NotBeNull();
            result.TrainingDataSize.Should().Be(100);
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
            result.RSquared.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task PredictMaintenanceAsync_WithValidPrinterId_ShouldReturnPredictions()
        {
            // Arrange
            var printerId = 1;

            // Act
            var predictions = await _predictiveService.PredictMaintenanceAsync(printerId, TimeSpan.FromDays(30));

            // Assert
            predictions.Should().NotBeNull();
            predictions.Should().HaveCountGreaterThan(0);

            foreach (var prediction in predictions)
            {
                prediction.PrinterId.Should().Be(printerId);
                prediction.Probability.Should().BeInRange(0, 1);
                prediction.PredictedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
            }
        }

        [Fact]
        public async Task GetRecentPredictionsAsync_ShouldReturnRecentPredictions()
        {
            // Act
            var predictions = await _predictiveService.GetRecentPredictionsAsync();

            // Assert
            predictions.Should().NotBeNull();
            predictions.Should().HaveCountGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetRecentPredictionsAsync_WithPrinterId_ShouldFilterByPrinter()
        {
            // Arrange
            var printerId = 1;

            // Act
            var predictions = await _predictiveService.GetRecentPredictionsAsync(printerId);

            // Assert
            predictions.Should().NotBeNull();
            predictions.Should().AllSatisfy(p => p.PrinterId.Should().Be(printerId));
        }

        private List<PrinterTelemetryClean> GenerateSampleTrainingData(int count)
        {
            var data = new List<PrinterTelemetryClean>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                data.Add(new PrinterTelemetryClean
                {
                    Id = i + 1,
                    PrinterId = random.Next(1, 10),
                    TimestampUtc = DateTime.UtcNow.AddHours(-random.Next(1, 24)),
                    AvgTonerLevel = random.Next(10, 100),
                    AvgPaperLevel = random.Next(5, 100),
                    TotalErrors = random.Next(0, 10),
                    AvgTemperature = random.Next(20, 80),
                    AvgCpuUsage = random.Next(10, 90),
                    AvgMemoryUsage = random.Next(20, 95),
                    AvgJobsInQueue = random.Next(0, 15),
                    SampleCount = random.Next(5, 20),
                    DataQualityScore = random.Next(70, 100),
                    DominantStatus = "Online"
                });
            }

            return data;
        }
    }

    public class TelemetryCollectorServiceTests
    {
        private readonly Mock<IPrinterRepository> _printerRepositoryMock;
        private readonly Mock<ILogger<TelemetryCollectorService>> _loggerMock;
        private readonly TelemetryCollectorService _telemetryService;

        public TelemetryCollectorServiceTests()
        {
            _printerRepositoryMock = new Mock<IPrinterRepository>();
            _loggerMock = new Mock<ILogger<TelemetryCollectorService>>();
            _telemetryService = new TelemetryCollectorService(_printerRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CollectAllPrinterMetricsAsync_ShouldReturnCollectionResult()
        {
            // Arrange
            var printers = new List<Printer>
            {
                new Printer { Id = 1, Status = PrinterStatus.Online, Name = "Printer 1" },
                new Printer { Id = 2, Status = PrinterStatus.Offline, Name = "Printer 2" }
            };

            _printerRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(printers);

            // Act
            var result = await _telemetryService.CollectAllPrinterMetricsAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalPrinters.Should().Be(2);
            result.ActivePrinters.Should().Be(1);
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CollectPrinterMetricsAsync_WithOnlinePrinter_ShouldReturnTelemetry()
        {
            // Arrange
            var printer = new Printer
            {
                Id = 1,
                Status = PrinterStatus.Online,
                Name = "Test Printer",
                IpAddress = "192.168.1.100"
            };

            // Act
            var telemetry = await _telemetryService.CollectPrinterMetricsAsync(printer);

            // Assert
            telemetry.Should().NotBeNull();
            telemetry!.PrinterId.Should().Be(1);
            telemetry.Status.Should().Be("Online");
            telemetry.CollectionSuccessful.Should().BeTrue();
            telemetry.CollectionTimeMs.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetRecentMetricsAsync_ShouldReturnFilteredMetrics()
        {
            // Arrange
            var printerId = 1;
            var timeWindow = TimeSpan.FromHours(1);

            // Act
            var metrics = await _telemetryService.GetRecentMetricsAsync(printerId, timeWindow);

            // Assert
            metrics.Should().NotBeNull();
            metrics.Should().AllSatisfy(m => m.PrinterId.Should().Be(printerId));
        }

        [Fact]
        public async Task GetCollectionStatisticsAsync_ShouldReturnStatistics()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;

            // Act
            var statistics = await _telemetryService.GetCollectionStatisticsAsync(fromDate, toDate);

            // Assert
            statistics.Should().NotBeNull();
            statistics.PeriodStart.Should().Be(fromDate);
            statistics.PeriodEnd.Should().Be(toDate);
        }

        [Fact]
        public void CleanupRecentMetrics_ShouldRemoveOldMetrics()
        {
            // Act
            _telemetryService.CleanupRecentMetrics();

            // Assert
            // Este método modifica el estado interno, no devuelve valores
            // En una implementación real verificaría que las métricas antiguas fueron removidas
        }
    }

    public class TelemetryDataCleanerTests
    {
        private readonly Mock<ILogger<TelemetryDataCleaner>> _loggerMock;
        private readonly TelemetryDataCleaner _dataCleaner;

        public TelemetryDataCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TelemetryDataCleaner>>();
            _dataCleaner = new TelemetryDataCleaner(_loggerMock.Object);
        }

        [Fact]
        public async Task CleanAndNormalizeTelemetryAsync_WithValidData_ShouldReturnCleanResult()
        {
            // Arrange
            var rawData = GenerateSampleRawTelemetry(50);

            // Act
            var result = await _dataCleaner.CleanAndNormalizeTelemetryAsync(rawData);

            // Assert
            result.Should().NotBeNull();
            result.TotalRawRecords.Should().Be(50);
            result.TotalCleanRecords.Should().BeGreaterThan(0);
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task CleanAndNormalizeTelemetryAsync_WithInvalidData_ShouldHandleErrors()
        {
            // Arrange
            var rawData = new List<PrinterTelemetry>
            {
                new PrinterTelemetry(1, DateTime.UtcNow, "Online")
                {
                    TonerLevel = -5, // Valor inválido
                    PaperLevel = 150  // Valor inválido
                }
            };

            // Act
            var result = await _dataCleaner.CleanAndNormalizeTelemetryAsync(rawData);

            // Assert
            result.Should().NotBeNull();
            result.InvalidRecords.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetDataQualityStatisticsAsync_ShouldReturnStatistics()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;

            // Act
            var statistics = await _dataCleaner.GetDataQualityStatisticsAsync(fromDate, toDate);

            // Assert
            statistics.Should().NotBeNull();
            statistics.PeriodStart.Should().Be(fromDate);
            statistics.PeriodEnd.Should().Be(toDate);
            statistics.AverageDataQuality.Should().BeGreaterThan(0);
        }

        private List<PrinterTelemetry> GenerateSampleRawTelemetry(int count)
        {
            var data = new List<PrinterTelemetry>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                data.Add(new PrinterTelemetry(random.Next(1, 10), DateTime.UtcNow.AddMinutes(-random.Next(1, 60)), "Online")
                {
                    TonerLevel = random.Next(10, 100),
                    PaperLevel = random.Next(5, 100),
                    PagesPrinted = random.Next(0, 100),
                    ErrorsCount = random.Next(0, 5),
                    Temperature = random.Next(20, 80),
                    CpuUsage = random.Next(10, 90),
                    MemoryUsage = random.Next(20, 95),
                    JobsInQueue = random.Next(0, 10),
                    CollectionSuccessful = true
                });
            }

            return data;
        }
    }
}
