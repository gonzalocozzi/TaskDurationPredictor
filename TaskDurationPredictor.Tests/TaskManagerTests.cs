namespace TaskDurationPredictor.Tests
{
    public class TaskManagerTests
    {
        private const string TestHistoryFile = "testHistory.json";

        public TaskManagerTests()
        {
            // Asegurarse de que el archivo de historial de pruebas no exista antes de comenzar.
            if (File.Exists(TestHistoryFile))
            {
                File.Delete(TestHistoryFile);
            }
        }

        [Fact]
        public void AddNewTaskHistory_ShouldAddTask()
        {
            // Arrange
            TaskManager manager = new TaskManager(TestHistoryFile);

            // Act
            manager.AddNewTaskHistory("Tarea1", 5.0);

            // Assert
            Assert.True(manager.HasTaskHistory("Tarea1"));
        }

        [Fact]
        public void GetAverageDuration_ShouldReturnCorrectAverage()
        {
            // Arrange
            TaskManager manager = new TaskManager(TestHistoryFile);
            manager.AddNewTaskHistory("Tarea1", 5.0);
            manager.UpdateTaskHistory("Tarea1", 10.0);

            // Act
            double average = manager.GetAverageDuration("Tarea1");

            // Assert
            Assert.Equal(7.5, average);
        }

        [Fact]
        public void UpdateTaskHistory_ShouldAddDurationToExistingTask()
        {
            // Arrange
            TaskManager manager = new TaskManager(TestHistoryFile);
            manager.AddNewTaskHistory("Tarea1", 5.0);

            // Act
            manager.UpdateTaskHistory("Tarea1", 7.0);

            // Assert
            double average = manager.GetAverageDuration("Tarea1");
            Assert.Equal(6.0, average); // Promedio de [5.0, 7.0]
        }

        [Fact]
        public void LoadTaskHistory_ShouldLoadFromFile()
        {
            // Arrange
            TaskManager manager = new TaskManager(TestHistoryFile);
            manager.AddNewTaskHistory("Tarea1", 5.0);

            // Act
            TaskManager newManager = new TaskManager(TestHistoryFile);
            bool hasTask = newManager.HasTaskHistory("Tarea1");

            // Assert
            Assert.True(hasTask);
            Assert.Equal(5.0, newManager.GetAverageDuration("Tarea1"));
        }

        [Fact]
        public void SimulateTaskWithPrediction_ShouldReportProgressAndEstimate()
        {
            // Arrange
            TaskManager manager = new TaskManager(TestHistoryFile);
            manager.AddNewTaskHistory("Tarea1", 5.0);
            bool progressReported = false;
            bool estimateReported = false;

            // Act
            manager.SimulateTaskWithPrediction("Tarea1", (progress, estimatedRemaining) =>
            {
                progressReported = progress > 0 && progress <= 100;
                estimateReported = estimatedRemaining >= 0;
            });

            // Assert
            Assert.True(progressReported);
            Assert.True(estimateReported);
        }

        [Fact]
        public void SimulateTaskWithoutPrediction_ShouldReportProgress()
        {
            // Arrange
            TaskManager manager = new TaskManager(TestHistoryFile);
            bool progressReported = false;

            // Act
            manager.SimulateTaskWithoutPrediction("Tarea1", progress =>
            {
                progressReported = progress > 0 && progress <= 100;
            });

            // Assert
            Assert.True(progressReported);
        }
    }
}