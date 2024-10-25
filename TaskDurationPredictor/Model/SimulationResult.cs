namespace TaskDurationPredictor.Model
{
    public class SimulationResult
    {
        public double Progress { get; set; }
        public double? EstimatedRemaining { get; set; }
        public double? ActualDuration { get; set; }
        public double? AverageDuration { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCancelled { get; set; }
    }
}