using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MonitorImpresoras.Application.Services;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Tests.Unit
{
    public class ModelRetrainingServiceTests
    {
        private readonly Mock<ILogger<ModelRetrainingService>> _loggerMock;
        private readonly Mock<IMaintenancePredictionRepository> _predictionRepositoryMock;
        private readonly Mock<IPredictionFeedbackRepository> _feedbackRepositoryMock;
        private readonly Mock<IPredictionTrainingDataRepository> _trainingDataRepositoryMock;
        private readonly ModelRetrainingService _retrainingService;

        public ModelRetrainingServiceTests()
        {
            _loggerMock = new Mock<ILogger<ModelRetrainingService>>();
            _predictionRepositoryMock = new Mock<IMaintenancePredictionRepository>();
            _feedbackRepositoryMock = new Mock<IPredictionFeedbackRepository>();
            _trainingDataRepositoryMock = new Mock<IPredictionTrainingDataRepository>();

            _retrainingService = new ModelRetrainingService(
                _loggerMock.Object,
                _predictionRepositoryMock.Object,
                _feedbackRepositoryMock.Object,
                _trainingDataRepositoryMock.Object);
        }

        [Fact]
        public async Task RetrainModelAsync_WithSufficientData_ShouldReturnRetrainingResult()
        {
            // Arrange
            var trainingData = GenerateSampleTrainingData(100);
            var feedback = GenerateSampleFeedback(50);

            _trainingDataRepositoryMock
                .Setup(x => x.GetReadyForTrainingAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(trainingData);

            _feedbackRepositoryMock
                .Setup(x => x.GetRecentFeedbackAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(feedback);

            // Act
            var result = await _retrainingService.RetrainModelAsync();

            // Assert
            result.Should().NotBeNull();
            result.TrainingDataSize.Should().Be(100);
            result.FeedbackDataSize.Should().Be(50);
            result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Fact]
        public async Task RetrainModelAsync_WithInsufficientData_ShouldReturnResultWithIssues()
        {
            // Arrange
            var trainingData = GenerateSampleTrainingData(10); // Datos insuficientes

            _trainingDataRepositoryMock
                .Setup(x => x.GetReadyForTrainingAsync(It.IsAny<DateTime>()))
                .ReturnsAsync(trainingData);

            // Act
            var result = await _retrainingService.RetrainModelAsync();

            // Assert
            result.Should().NotBeNull();
            result.IssuesFound.Should().Contain("Datos de entrenamiento insuficientes");
            result.ModelUpdated.Should().BeFalse();
        }

        [Fact]
        public async Task ProcessFeedbackAsync_WithValidFeedback_ShouldProcessSuccessfully()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, false, "testuser")
            {
                Comment = "El fallo ocurrió antes de lo previsto",
                Id = 1
            };

            var prediction = new MaintenancePrediction(1, "TonerDepletion", 0.8m)
            {
                Id = 1,
                InputData = "{\"AvgTonerLevel\": 25}",
                PredictionType = "TonerDepletion"
            };

            _predictionRepositoryMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(prediction);

            // Act
            await _retrainingService.ProcessFeedbackAsync(feedback);

            // Assert
            _trainingDataRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<PredictionTrainingData>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAdvancedStatisticsAsync_ShouldReturnStatistics()
        {
            // Act
            var statistics = await _retrainingService.GetAdvancedStatisticsAsync();

            // Assert
            statistics.Should().NotBeNull();
            statistics.TotalFeedback.Should().BeGreaterThanOrEqualTo(0);
            statistics.AccuracyByType.Should().NotBeNull();
            statistics.AverageAnticipationDays.Should().BeGreaterThan(0);
        }

        private List<PredictionTrainingData> GenerateSampleTrainingData(int count)
        {
            var data = new List<PredictionTrainingData>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                data.Add(new PredictionTrainingData(
                    feedbackId: random.Next(1, 100),
                    predictionId: random.Next(1, 100),
                    inputData: "{\"AvgTonerLevel\": 50}",
                    originalProbability: random.Next(10, 90) / 100.0m,
                    predictionType: "TonerDepletion")
                {
                    Id = i + 1,
                    ActualDaysUntilEvent = random.Next(1, 30),
                    EventOccurred = true,
                    IsReadyForTraining = true,
                    TrainingWeight = 1.0m
                });
            }

            return data;
        }

        private List<PredictionFeedback> GenerateSampleFeedback(int count)
        {
            var feedback = new List<PredictionFeedback>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                feedback.Add(new PredictionFeedback(
                    predictionId: random.Next(1, 100),
                    isCorrect: random.Next(2) == 1,
                    createdBy: $"user{random.Next(1, 10)}@empresa.com")
                {
                    Id = i + 1,
                    Comment = $"Feedback {i + 1}",
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                });
            }

            return feedback;
        }
    }

    public class PredictiveMaintenanceServiceFeedbackTests
    {
        private readonly Mock<ILogger<PredictiveMaintenanceService>> _loggerMock;
        private readonly Mock<IModelRetrainingService> _retrainingServiceMock;
        private readonly Mock<IPredictionFeedbackRepository> _feedbackRepositoryMock;
        private readonly PredictiveMaintenanceService _predictiveService;

        public PredictiveMaintenanceServiceFeedbackTests()
        {
            _loggerMock = new Mock<ILogger<PredictiveMaintenanceService>>();
            _retrainingServiceMock = new Mock<IModelRetrainingService>();
            _feedbackRepositoryMock = new Mock<IPredictionFeedbackRepository>();

            _predictiveService = new PredictiveMaintenanceService(
                _loggerMock.Object,
                _retrainingServiceMock.Object,
                _feedbackRepositoryMock.Object);
        }

        [Fact]
        public async Task ProcessFeedbackAsync_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var predictionId = 1L;
            var isCorrect = false;
            var comment = "El fallo ocurrió antes";
            var userId = "testuser";

            _retrainingServiceMock
                .Setup(x => x.ProcessFeedbackAsync(It.IsAny<PredictionFeedback>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _predictiveService.ProcessFeedbackAsync(predictionId, isCorrect, comment, userId);

            // Assert
            result.Should().BeTrue();
            _retrainingServiceMock.Verify(
                x => x.ProcessFeedbackAsync(It.Is<PredictionFeedback>(
                    f => f.PredictionId == predictionId &&
                         f.IsCorrect == isCorrect &&
                         f.Comment == comment &&
                         f.CreatedBy == userId)),
                Times.Once);
        }

        [Fact]
        public async Task GetAdvancedStatisticsAsync_ShouldReturnStatistics()
        {
            // Arrange
            var expectedStats = new AdvancedPredictionStatistics
            {
                OverallAccuracy = 0.85m,
                TotalFeedback = 100,
                AverageAnticipationDays = 2.5m
            };

            _retrainingServiceMock
                .Setup(x => x.GetAdvancedStatisticsAsync(null, null))
                .ReturnsAsync(expectedStats);

            // Act
            var statistics = await _predictiveService.GetAdvancedStatisticsAsync();

            // Assert
            statistics.Should().NotBeNull();
            statistics.OverallAccuracy.Should().Be(0.85m);
            statistics.TotalFeedback.Should().Be(100);
        }

        [Fact]
        public async Task RetrainModelAsync_ShouldReturnRetrainingResult()
        {
            // Arrange
            var expectedResult = new RetrainingResult
            {
                ModelUpdated = true,
                TrainingDataSize = 200,
                ImprovementFromPrevious = 0.05m
            };

            _retrainingServiceMock
                .Setup(x => x.RetrainModelAsync())
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _predictiveService.RetrainModelAsync();

            // Assert
            result.Should().NotBeNull();
            result.ModelUpdated.Should().BeTrue();
            result.TrainingDataSize.Should().Be(200);
        }
    }

    public class PredictionFeedbackTests
    {
        [Fact]
        public void PredictionFeedback_WithValidData_ShouldCalculateQuality()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, false, "testuser")
            {
                Comment = "Este es un comentario detallado sobre la predicción que fue incorrecta porque el fallo ocurrió antes de lo previsto.",
                ProposedCorrection = "La predicción debería haber sido 2 días antes"
            };

            // Act
            var quality = feedback.GetQuality();

            // Assert
            quality.Should().Be(FeedbackQuality.High);
        }

        [Fact]
        public void PredictionFeedback_WithShortComment_ShouldHaveMediumQuality()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, true, "testuser")
            {
                Comment = "OK"
            };

            // Act
            var quality = feedback.GetQuality();

            // Assert
            quality.Should().Be(FeedbackQuality.Medium);
        }

        [Fact]
        public void PredictionFeedback_WithoutComment_ShouldHaveLowQuality()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, true, "testuser");

            // Act
            var quality = feedback.GetQuality();

            // Assert
            quality.Should().Be(FeedbackQuality.Low);
        }

        [Fact]
        public void PredictionFeedback_TimeSinceCreation_ShouldCalculateCorrectly()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, true, "testuser")
            {
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            };

            // Act
            var timeSinceCreation = feedback.TimeSinceCreation;

            // Assert
            timeSinceCreation.Should().BeCloseTo(TimeSpan.FromHours(2), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void PredictionFeedback_IsRecent_ShouldReturnTrueForRecentFeedback()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, true, "testuser")
            {
                CreatedAt = DateTime.UtcNow.AddHours(-20)
            };

            // Act
            var isRecent = feedback.IsRecent;

            // Assert
            isRecent.Should().BeTrue();
        }

        [Fact]
        public void PredictionFeedback_IsRecent_ShouldReturnFalseForOldFeedback()
        {
            // Arrange
            var feedback = new PredictionFeedback(1, true, "testuser")
            {
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            // Act
            var isRecent = feedback.IsRecent;

            // Assert
            isRecent.Should().BeFalse();
        }
    }
}
