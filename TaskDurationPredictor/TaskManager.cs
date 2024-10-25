using TaskDurationPredictor.Repository;

namespace TaskDurationPredictor
{
    public class TaskManager
    {

        private readonly TaskHistoryRepository _repository;
        private static readonly Random random = new(); // Generador de números aleatorios

        public TaskManager(TaskHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task SimulateTaskAsync(string taskName,
                                            Action<double, double?> onProgressUpdated,
                                            Action<double> averageDurationMessage,
                                            Action<double> actualDurationMessage,
                                            CancellationToken cancellationToken)
        {
            double actualDuration;
            double averageDuration = 0;
            bool usePrediction = _repository.HasTaskHistory(taskName);

            if (usePrediction)
            {
                averageDuration = _repository.GetAverageDuration(taskName);
                double randomFactor = random.NextDouble() * 1.5 + 0.1;
                actualDuration = averageDuration * randomFactor;
                averageDurationMessage.Invoke(averageDuration);
            }
            else
            {
                actualDuration = random.Next(5, 30); // Duración aleatoria sin predicción
            }

            double progress = 0;
            double elapsed = 0;

            while (progress < 100)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Simulación cancelada.");
                    break;
                }

                await Task.Delay(500); // Simula medio segundo de trabajo
                elapsed += 0.5;
                progress = elapsed / actualDuration * 100;
                if (progress > 100) progress = 100;

                double? estimatedRemaining = usePrediction ? actualDuration - elapsed : null;
                if (estimatedRemaining.HasValue && estimatedRemaining < 0) estimatedRemaining = 0;
                onProgressUpdated?.Invoke(progress, estimatedRemaining); // Llama al callback
            }

            if (progress == 100)
            {
                actualDurationMessage.Invoke(actualDuration);
                _repository.AddOrUpdateTaskHistory(taskName, actualDuration);
            }
        }

        public async Task SimulateTaskAsyncV2(string taskName,
                                            Action<double, double?> onProgressUpdated,
                                            Action<double> averageDurationMessage,
                                            Action<double> actualDurationMessage,
                                            CancellationToken cancellationToken)
        {
            double actualDuration;
            double averageDuration = 0;
            bool usePrediction = _repository.HasTaskHistory(taskName);

            if (usePrediction)
            {
                averageDuration = _repository.GetAverageDuration(taskName);
                double randomFactor = random.NextDouble() * 1.5 + 0.1;
                actualDuration = averageDuration * randomFactor;
                averageDurationMessage.Invoke(averageDuration);
            }
            else
            {
                actualDuration = random.Next(5, 30); // Duración aleatoria sin predicción
            }

            double progress = 0;
            double elapsed = 0;

            while (progress < 100)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Simulación cancelada.");
                    break;
                }

                var randomElapsedTime = random.Next(10, 100);
                await Task.Delay(randomElapsedTime); // Simula medio segundo de trabajo
                elapsed += randomElapsedTime / 1000d;
                Console.WriteLine(elapsed);
                progress++;

                var elapsedTimePrediction = 100 * elapsed / progress;
                var currentAverageDuration = (averageDuration + elapsedTimePrediction) / 2;

                double? estimatedRemaining = usePrediction ? currentAverageDuration - elapsed : null;
                if (estimatedRemaining.HasValue && estimatedRemaining < 0) estimatedRemaining = 0;
                onProgressUpdated?.Invoke(progress, estimatedRemaining); // Llama al callback
            }

            if (progress == 100)
            {
                actualDurationMessage.Invoke(elapsed);
                _repository.AddOrUpdateTaskHistory(taskName, elapsed);
            }
        }
    }
}