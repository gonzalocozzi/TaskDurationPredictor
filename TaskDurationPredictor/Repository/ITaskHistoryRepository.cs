namespace TaskDurationPredictor.Repository
{
    public interface ITaskHistoryRepository
    {
        void AddNewTaskHistory(string taskName, double duration);
        void AddOrUpdateTaskHistory(string taskName, double actualDuration);
        double GetAverageDuration(string taskName);
        bool HasTaskHistory(string taskName);
        void SaveTaskHistory();
        void UpdateTaskHistory(string taskName, double duration);
    }
}