using System.Collections.Concurrent;
using TaskDurationPredictor.Helper;
using TaskDurationPredictor.Model;
using TaskDurationPredictor.Repository;

namespace TaskDurationPredictor.Domain
{
    public class TaskManager
    {
        private readonly ITaskHistoryRepository _repository;
        private readonly BlockingCollection<SimulationResult> _resultQueue;
        private static readonly Random _random = new();

        private const double MIN_RANDOM_FACTOR = 0.5;
        private const double MAX_RANDOM_FACTOR = 2.5;
        private const int MIN_BASE_DURATION = 3;
        private const int MAX_BASE_DURATION = 45;

        // Factores para el sistema de estimación adaptativa
        private const int PROGRESS_WINDOW_SIZE = 5;  // Tamaño de la ventana para calcular la velocidad reciente
        private const double HISTORICAL_WEIGHT = 0.6; // Peso de la estimación histórica vs. la actual
        private const double ADAPTATION_RATE = 0.2;   // Qué tan rápido se adapta la estimación

        public TaskManager(ITaskHistoryRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _resultQueue = [];
        }
        public async Task SimulateTaskAsync(string taskName,
                                            Action<double, double?> onProgressUpdated,
                                            Action<double> averageDurationMessage,
                                            Action<double> actualDurationMessage,
                                            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(taskName))
                throw new ArgumentNullException(nameof(taskName));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                var calculationTask = Task.Run(() =>
                    CalculationThreadAsync(taskName, linkedCts.Token), linkedCts.Token);

                var presentationTask = Task.Run(() =>
                    PresentationThread(onProgressUpdated, averageDurationMessage, actualDurationMessage, linkedCts.Token),
                    linkedCts.Token);

                await Task.WhenAll(calculationTask, presentationTask);
            }
            finally
            {
                _resultQueue.Dispose();
            }
        }

        private async Task CalculationThreadAsync(string taskName, CancellationToken cancellationToken)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                bool usePrediction = _repository.HasTaskHistory(taskName);

                if (!cancellationToken.IsCancellationRequested && usePrediction)
                {
                    _resultQueue.Add(new SimulationResult
                    {
                        AverageDuration = _repository.GetAverageDuration(taskName)
                    },
                    cancellationToken);
                }

                for (int i = 1; i <= 100; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Simulate random delay up to 2 seconds
                    int delay = _random.Next(1, 201);
                    await Task.Delay(delay, cancellationToken);

                    // Calculate elapsed time
                    var elapsedTime = DateTime.UtcNow - startTime;

                    // Estimate remaining time in seconds
                    var estimatedRemainingTime = elapsedTime.TotalSeconds / i * (100 - i);

                    // Add progress and estimated remaining time to the result queue
                    _resultQueue.Add(new SimulationResult
                    {
                        Progress = i,
                        EstimatedRemaining = estimatedRemainingTime
                    }, cancellationToken);
                }

                // Calculate total elapsed time
                var totalElapsedTime = (DateTime.UtcNow - startTime).TotalSeconds;

                if (!cancellationToken.IsCancellationRequested)
                {
                    _resultQueue.Add(new SimulationResult
                    {
                        ActualDuration = totalElapsedTime,
                        IsCompleted = true,
                        Progress = 100
                    }, cancellationToken);

                    await Task.Run(() => _repository.AddOrUpdateTaskHistory(taskName, totalElapsedTime),
                        cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _resultQueue.Add(new SimulationResult { IsCancelled = true }, cancellationToken);
            }
            finally
            {
                _resultQueue.CompleteAdding();
            }
        }

        
        private void PresentationThread(Action<double, double?> onProgressUpdated,
                                        Action<double> averageDurationMessage,
                                        Action<double> actualDurationMessage,
                                        CancellationToken cancellationToken)
        {
            try
            {
                foreach (var result in _resultQueue.GetConsumingEnumerable(cancellationToken))
                {
                    if (result.IsCancelled)
                    {
                        Console.WriteLine("Simulación cancelada.");
                        break;
                    }

                    if (result.AverageDuration.HasValue)
                    {
                        averageDurationMessage?.Invoke(result.AverageDuration.Value);
                    }

                    if (result.Progress > 0)
                    {
                        onProgressUpdated?.Invoke(result.Progress, result.EstimatedRemaining);
                    }

                    if (result.IsCompleted && result.ActualDuration.HasValue)
                    {
                        actualDurationMessage?.Invoke(result.ActualDuration.Value);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Simulación cancelada.");
            }
        }
    }
}