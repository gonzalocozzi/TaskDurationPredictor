using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TaskDurationPredictor.Model;
using TaskDurationPredictor.Repository;
using Xunit;

namespace TaskDurationPredictor.Tests
{
    public class TaskHistoryRepositoryTests : IDisposable
    {
        private readonly string _testFileName;
        private readonly TaskHistoryRepository _repository;

        public TaskHistoryRepositoryTests()
        {
            _testFileName = "test_task_history.json";
            _repository = new TaskHistoryRepository(_testFileName);
        }

        [Fact]
        public void AddNewTaskHistory_ShouldAddTaskHistory()
        {
            // Arrange
            string taskName = "TestTask";
            double duration = 10.0;

            // Act
            _repository.AddNewTaskHistory(taskName, duration);

            // Assert
            Assert.True(_repository.HasTaskHistory(taskName));
            Assert.Equal(duration, _repository.GetAverageDuration(taskName));
        }

        [Fact]
        public void UpdateTaskHistory_ShouldUpdateExistingTaskHistory()
        {
            // Arrange
            string taskName = "TestTask";
            double initialDuration = 10.0;
            double newDuration = 20.0;
            _repository.AddNewTaskHistory(taskName, initialDuration);

            // Act
            _repository.UpdateTaskHistory(taskName, newDuration);

            // Assert
            Assert.True(_repository.HasTaskHistory(taskName));
            Assert.Equal((initialDuration + newDuration) / 2, _repository.GetAverageDuration(taskName));
        }

        [Fact]
        public void GetAverageDuration_ShouldReturnCorrectAverage()
        {
            // Arrange
            string taskName = "TestTask";
            double duration1 = 10.0;
            double duration2 = 20.0;
            _repository.AddNewTaskHistory(taskName, duration1);
            _repository.UpdateTaskHistory(taskName, duration2);

            // Act
            double averageDuration = _repository.GetAverageDuration(taskName);

            // Assert
            Assert.Equal((duration1 + duration2) / 2, averageDuration);
        }

        [Fact]
        public void HasTaskHistory_ShouldReturnTrueForExistingTask()
        {
            // Arrange
            string taskName = "TestTask";
            _repository.AddNewTaskHistory(taskName, 10.0);

            // Act
            bool hasHistory = _repository.HasTaskHistory(taskName);

            // Assert
            Assert.True(hasHistory);
        }

        [Fact]
        public void HasTaskHistory_ShouldReturnFalseForNonExistingTask()
        {
            // Arrange
            string taskName = "NonExistingTask";

            // Act
            bool hasHistory = _repository.HasTaskHistory(taskName);

            // Assert
            Assert.False(hasHistory);
        }

        [Fact]
        public void AddOrUpdateTaskHistory_ShouldAddOrUpdateTaskHistory()
        {
            // Arrange
            string taskName = "TestTask";
            double initialDuration = 10.0;
            double newDuration = 20.0;

            // Act
            _repository.AddOrUpdateTaskHistory(taskName, initialDuration);
            _repository.AddOrUpdateTaskHistory(taskName, newDuration);

            // Assert
            Assert.True(_repository.HasTaskHistory(taskName));
            Assert.Equal((initialDuration + newDuration) / 2, _repository.GetAverageDuration(taskName));
        }

        [Fact]
        public void SaveTaskHistory_ShouldPersistData()
        {
            // Arrange
            string taskName = "TestTask";
            double duration = 10.0;
            _repository.AddNewTaskHistory(taskName, duration);

            // Act
            _repository.SaveTaskHistory();

            // Assert
            var loadedRepository = new TaskHistoryRepository(_testFileName);
            Assert.True(loadedRepository.HasTaskHistory(taskName));
            Assert.Equal(duration, loadedRepository.GetAverageDuration(taskName));
        }

        public void Dispose()
        {
            if (File.Exists(_testFileName))
            {
                File.Delete(_testFileName);
            }
        }
    }
}
