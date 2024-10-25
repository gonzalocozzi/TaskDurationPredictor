namespace TaskDurationPredictor.Model
{
    public class TaskHistory
    {
        public required string TaskName { get; set; }
        public required List<double> Durations { get; set; }
    }
}