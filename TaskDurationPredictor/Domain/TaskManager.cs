using System.Collections.Concurrent;
using TaskDurationPredictor.Helper;
using TaskDurationPredictor.Model;
using TaskDurationPredictor.Repository;

namespace TaskDurationPredictor.Domain
{
    public class TaskManager
    {
        private readonly TaskHistoryRepository _repository;
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

        public TaskManager(TaskHistoryRepository repository)
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
                var simulationParams = CalculateSimulationParameters(taskName);

                if (!cancellationToken.IsCancellationRequested && simulationParams.UsePrediction)
                {
                    _resultQueue.Add(new SimulationResult
                    {
                        AverageDuration = simulationParams.AverageDuration
                    },
                    cancellationToken);
                }

                var finalProgress = await SimulateProgressAsync(simulationParams, cancellationToken);

                if (!cancellationToken.IsCancellationRequested && finalProgress >= 100)
                {
                    _resultQueue.Add(new SimulationResult
                    {
                        ActualDuration = simulationParams.ActualDuration,
                        IsCompleted = true,
                        Progress = 100
                    }, cancellationToken);

                    await Task.Run(() => _repository.AddOrUpdateTaskHistory(taskName, simulationParams.ActualDuration),
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

        private static (double ActualDuration, double AverageDuration, bool UsePrediction, double Progress) CalculateSimulationParameters(string taskName)
        {
            double actualDuration;
            double averageDuration = 0;
            bool usePrediction = TaskHistoryRepository.HasTaskHistory(taskName);

            if (usePrediction)
            {
                averageDuration = TaskHistoryRepository.GetAverageDuration(taskName);

                // Aplicamos múltiples factores de aleatoriedad
                double baseRandomFactor = _random.NextDouble() * (MAX_RANDOM_FACTOR - MIN_RANDOM_FACTOR) + MIN_RANDOM_FACTOR;

                // Añadimos una variación adicional basada en una distribución normal
                double normalDistribution = RandomHelper.NormalDistributionRandom();
                double combinedFactor = baseRandomFactor * (1 + normalDistribution * 0.2);

                actualDuration = averageDuration * combinedFactor;
            }
            else
            {
                // Para duraciones sin predicción, usamos una distribución más variada
                double baseValue = _random.Next(MIN_BASE_DURATION, MAX_BASE_DURATION);
                double variationFactor = 1 + (_random.NextDouble() - 0.5) * 0.6; // ±30% variación
                actualDuration = baseValue * variationFactor;
            }

            return (actualDuration, averageDuration, usePrediction, 0);
        }

        private async Task<double> SimulateProgressAsync((double ActualDuration, double AverageDuration, bool UsePrediction, double Progress) parameters,
                                                         CancellationToken cancellationToken)
        {
            double elapsed = 0;
            double progress = parameters.Progress;
            double progressVariability = 1.0;

            var progressTracker = new ProgressTracker(
                parameters.UsePrediction ? parameters.AverageDuration : parameters.ActualDuration,
                PROGRESS_WINDOW_SIZE,
                ADAPTATION_RATE,
                HISTORICAL_WEIGHT
            );

            while (progress < 100 && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(500, cancellationToken);

                // Variabilidad en el tiempo transcurrido
                double timeIncrement = 0.5 * (1 + (_random.NextDouble() - 0.5) * 0.2);
                elapsed += timeIncrement;

                // Actualizar progreso con variabilidad
                progressVariability = Math.Max(0.8, Math.Min(1.2,
                    progressVariability + (_random.NextDouble() - 0.5) * 0.1));
                progress = Math.Min(elapsed / parameters.ActualDuration * 100 * progressVariability, 100);

                // Actualizar el tracker y obtener nueva estimación
                progressTracker.AddProgressPoint(progress, elapsed);
                double currentEstimate = progressTracker.UpdateEstimate(progress, elapsed);

                // Calcular tiempo restante estimado
                double? estimatedRemaining = null;
                if (parameters.UsePrediction)
                {
                    estimatedRemaining = Math.Max(currentEstimate - elapsed, 0);
                }

                _resultQueue.Add(new SimulationResult
                {
                    Progress = progress,
                    EstimatedRemaining = estimatedRemaining
                },
                cancellationToken);
            }

            return progress;
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