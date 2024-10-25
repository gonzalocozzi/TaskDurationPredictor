namespace TaskDurationPredictor.Domain
{
    public class ProgressTracker
    {
        private readonly Queue<(double Progress, double Time)> _recentProgress;
        private readonly int _windowSize;
        private double _currentEstimate;
        private double _adaptationRate;
        private double _historicalWeight;

        public ProgressTracker(double initialEstimate, int windowSize,
            double adaptationRate, double historicalWeight)
        {
            _recentProgress = new Queue<(double, double)>();
            _windowSize = windowSize;
            _currentEstimate = initialEstimate;
            _adaptationRate = adaptationRate;
            _historicalWeight = historicalWeight;
        }

        public void AddProgressPoint(double progress, double time)
        {
            _recentProgress.Enqueue((progress, time));
            if (_recentProgress.Count > _windowSize)
            {
                _recentProgress.Dequeue();
            }
        }

        public double GetCurrentVelocity()
        {
            if (_recentProgress.Count < 2) return 1.0;

            var progressPoints = _recentProgress.ToArray();
            var recentProgressRate = 0.0;

            for (int i = 1; i < progressPoints.Length; i++)
            {
                var progressDelta = progressPoints[i].Progress - progressPoints[i - 1].Progress;
                var timeDelta = progressPoints[i].Time - progressPoints[i - 1].Time;
                if (timeDelta > 0)
                {
                    recentProgressRate += progressDelta / timeDelta;
                }
            }

            return recentProgressRate / (progressPoints.Length - 1);
        }

        public double UpdateEstimate(double progress, double elapsed)
        {
            if (progress < 1) return _currentEstimate;

            double currentVelocity = GetCurrentVelocity();
            if (currentVelocity <= 0) return _currentEstimate;

            // Calcula el tiempo restante basado en la velocidad actual
            double remainingProgress = 100 - progress;
            double estimatedRemainingTime = remainingProgress / currentVelocity;

            // Calcula el tiempo total estimado
            double newEstimate = elapsed + estimatedRemainingTime;

            // Combina la nueva estimación con la histórica usando los pesos
            _currentEstimate = _currentEstimate * _historicalWeight +
                             newEstimate * (1 - _historicalWeight);

            // Ajusta gradualmente el peso histórico
            _historicalWeight = Math.Max(0.2, _historicalWeight - _adaptationRate);

            return _currentEstimate;
        }
    }
}