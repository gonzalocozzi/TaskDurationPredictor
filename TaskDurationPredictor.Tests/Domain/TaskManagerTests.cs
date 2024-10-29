using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using TaskDurationPredictor.Domain;
using TaskDurationPredictor.Model;
using TaskDurationPredictor.Repository;
using Xunit;

namespace TaskDurationPredictor.Tests
{
    public class TaskManagerTests
    {
        private readonly Mock<ITaskHistoryRepository> _mockRepository;
        private readonly TaskManager _taskManager;

        public TaskManagerTests()
        {
            _mockRepository = new Mock<ITaskHistoryRepository>();
            _taskManager = new TaskManager(_mockRepository.Object);
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldThrowArgumentNullException_WhenTaskNameIsNull()
        {
            // Arrange
            string taskName = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _taskManager.SimulateTaskAsync(taskName, null, null, null, CancellationToken.None));
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldUsePrediction_WhenTaskHistoryExists()
        {
            // Arrange
            string taskName = "TestTask";
            _mockRepository.Setup(r => r.HasTaskHistory(taskName)).Returns(true);
            _mockRepository.Setup(r => r.GetAverageDuration(taskName)).Returns(10.0);

            // Act
            await _taskManager.SimulateTaskAsync(taskName, null, null, null, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.GetAverageDuration(taskName), Times.Once);
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldNotUsePrediction_WhenTaskHistoryDoesNotExist()
        {
            // Arrange
            string taskName = "TestTask";
            _mockRepository.Setup(r => r.HasTaskHistory(taskName)).Returns(false);

            // Act
            await _taskManager.SimulateTaskAsync(taskName, null, null, null, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.GetAverageDuration(taskName), Times.Never);
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldUpdateTaskHistory_WhenSimulationCompletes()
        {
            // Arrange
            string taskName = "TestTask";
            _mockRepository.Setup(r => r.HasTaskHistory(taskName)).Returns(false);

            // Act
            await _taskManager.SimulateTaskAsync(taskName, null, null, null, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.AddOrUpdateTaskHistory(taskName, It.IsAny<double>()), Times.Once);
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldInvokeCallbacks_WhenSimulationProgresses()
        {
            // Arrange
            string taskName = "TestTask";
            _mockRepository.Setup(r => r.HasTaskHistory(taskName)).Returns(false);

            double progressReported = 0;
            double? estimatedRemainingReported = null;
            double averageDurationReported = 0;
            double actualDurationReported = 0;

            Action<double, double?> onProgressUpdated = (progress, estimatedRemaining) =>
            {
                progressReported = progress;
                estimatedRemainingReported = estimatedRemaining;
            };

            Action<double> averageDurationMessage = (averageDuration) =>
            {
                averageDurationReported = averageDuration;
            };

            Action<double> actualDurationMessage = (actualDuration) =>
            {
                actualDurationReported = actualDuration;
            };

            // Act
            await _taskManager.SimulateTaskAsync(taskName, onProgressUpdated, averageDurationMessage, actualDurationMessage, CancellationToken.None);

            // Assert
            Assert.True(progressReported > 0);
            Assert.Equal(0, averageDurationReported); // No history, so average duration should not be reported
            Assert.True(actualDurationReported > 0);
        }
    }
}
