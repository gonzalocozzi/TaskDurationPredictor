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
        public async Task SimulateTaskAsync_ShouldThrowArgumentNullException_WhenTaskNameIsEmpty()
        {
            // Arrange
            string taskName = string.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _taskManager.SimulateTaskAsync(taskName, null, null, null, CancellationToken.None));
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldCallRepositoryAddOrUpdateTaskHistory_WhenSimulationCompletes()
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
        public async Task SimulateTaskAsync_ShouldNotCallRepositoryAddOrUpdateTaskHistory_WhenSimulationIsCancelled()
        {
            // Arrange
            string taskName = "TestTask";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                _taskManager.SimulateTaskAsync(taskName, null, null, null, cts.Token));

            // Assert
            _mockRepository.Verify(r => r.AddOrUpdateTaskHistory(taskName, It.IsAny<double>()), Times.Never);
        }

        [Fact]
        public async Task SimulateTaskAsync_ShouldUpdateProgress_WhenSimulationIsRunning()
        {
            // Arrange
            string taskName = "TestTask";
            double progressReported = 0;
            _mockRepository.Setup(r => r.HasTaskHistory(taskName)).Returns(false);

            // Act
            await _taskManager.SimulateTaskAsync(taskName,
                (progress, estimatedRemaining) => progressReported = progress,
                null, null, CancellationToken.None);

            // Assert
            Assert.True(progressReported > 0);
        }

        
    }
}
