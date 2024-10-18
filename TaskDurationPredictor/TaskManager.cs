using System.Text.Json;

namespace TaskDurationPredictor
{
    public class TaskManager
    {
        private readonly string _historyFile;
        private List<TaskHistory> _taskHistories;

        public TaskManager(string historyFile)
        {
            _historyFile = historyFile;
            _taskHistories = [];
            LoadTaskHistory();
        }

        public bool HasTaskHistory(string taskName)
        {
            return _taskHistories.Any(t => t.TaskName == taskName);
        }

        public double GetAverageDuration(string taskName)
        {
            var taskHistory = _taskHistories.First(t => t.TaskName == taskName);
            return taskHistory.Durations.Average();
        }

        public void SimulateTaskWithPrediction(string taskName, Action<int, double> reportProgress)
        {
            if (!HasTaskHistory(taskName))
                throw new InvalidOperationException("No hay datos históricos para esta tarea.");

            double averageDuration = GetAverageDuration(taskName);
            DateTime startTime = DateTime.Now;

            for (int i = 1; i <= 100; i++) // Simular una tarea
            {
                Thread.Sleep(100); // Simular trabajo
                double elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
                double estimatedRemaining = Math.Max(averageDuration - elapsedSeconds, 0);

                // Notificar el progreso y el tiempo estimado restante
                reportProgress(i, estimatedRemaining);
            }

            double actualDuration = (DateTime.Now - startTime).TotalSeconds;
            UpdateTaskHistory(taskName, actualDuration);
        }

        public void SimulateTaskWithoutPrediction(string taskName, Action<int> reportProgress)
        {
            DateTime startTime = DateTime.Now;

            for (int i = 1; i <= 100; i++) // Simular una tarea
            {
                Thread.Sleep(100); // Simular trabajo
                reportProgress(i); // Notificar el progreso
            }

            double actualDuration = (DateTime.Now - startTime).TotalSeconds;
            AddNewTaskHistory(taskName, actualDuration);
        }

        private void LoadTaskHistory()
        {
            if (File.Exists(_historyFile))
            {
                string json = File.ReadAllText(_historyFile);
                _taskHistories = JsonSerializer.Deserialize<List<TaskHistory>>(json) ?? [];
            }
        }

        private void SaveTaskHistory()
        {
            string json = JsonSerializer.Serialize(_taskHistories, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_historyFile, json);
        }

        public void UpdateTaskHistory(string taskName, double duration)
        {
            var taskHistory = _taskHistories.FirstOrDefault(t => t.TaskName == taskName);
            if (taskHistory != null)
            {
                taskHistory.Durations.Add(duration);
            }
            else
            {
                AddNewTaskHistory(taskName, duration);
            }
            SaveTaskHistory();
        }

        public void AddNewTaskHistory(string taskName, double duration)
        {
            _taskHistories.Add(new TaskHistory
            {
                TaskName = taskName,
                Durations = [duration]
            });
            SaveTaskHistory();
        }
    }
}