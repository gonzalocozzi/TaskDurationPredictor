using System.Text.Json;

namespace TaskDurationPredictor.Repository
{
    public class TaskHistoryRepository
    {
        private readonly string historyFileName;
        private static List<TaskHistory> taskHistories = [];

        public TaskHistoryRepository(string historyFileName)
        {
            this.historyFileName = historyFileName;
            LoadTaskHistory();
        }

        public void SaveTaskHistory()
        {
            string json = JsonSerializer.Serialize(taskHistories);
            File.WriteAllText(historyFileName, json);
        }

        public void AddNewTaskHistory(string taskName, double duration)
        {
            taskHistories.Add(new TaskHistory
            {
                TaskName = taskName,
                Durations = [duration]
            });
            SaveTaskHistory();
        }

        public void UpdateTaskHistory(string taskName, double duration)
        {
            TaskHistory history = taskHistories.Find(t => t.TaskName == taskName);
            if (history != null)
            {
                history.Durations.Add(duration);
                SaveTaskHistory();
            }
        }

        public static double GetAverageDuration(string taskName)
        {
            TaskHistory history = taskHistories.Find(t => t.TaskName == taskName);
            if (history != null && history.Durations.Count > 0)
            {
                double total = 0;
                foreach (var duration in history.Durations)
                {
                    total += duration;
                }
                return total / history.Durations.Count;
            }
            return 0;
        }

        public static bool HasTaskHistory(string taskName)
        {
            return taskHistories.Exists(t => t.TaskName == taskName);
        }

        private void LoadTaskHistory()
        {
            if (File.Exists(historyFileName))
            {
                string json = File.ReadAllText(historyFileName);
                taskHistories = !string.IsNullOrEmpty(json) ? JsonSerializer.Deserialize<List<TaskHistory>>(json) : [];
            }
        }

        public void AddOrUpdateTaskHistory(string taskName, double actualDuration)
        {
            if (HasTaskHistory(taskName))
            {
                UpdateTaskHistory(taskName, actualDuration); // Actualiza historial con predicción
            }
            else
            {
                AddNewTaskHistory(taskName, actualDuration); // Actualiza historial sin predicción
            }
        }
    }
}