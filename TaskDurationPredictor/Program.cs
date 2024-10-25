using TaskDurationPredictor.Repository;

namespace TaskDurationPredictor
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            TaskHistoryRepository repository = new("taskHistory.json");
            TaskManager taskManager = new(repository);
            CancellationTokenSource cts = new();

            Console.WriteLine("Ingresa el nombre de la tarea:");
            string taskName = Console.ReadLine();

            Console.WriteLine("Simulando tarea...");
            await taskManager.SimulateTaskAsync(taskName,
            (progress, estimatedRemaining) =>
            {
                string message = estimatedRemaining.HasValue ? $"Progreso: {progress:F2}% | Tiempo estimado restante: {estimatedRemaining:F2} segundos" : $"Progreso: {progress:F2}%";
                Console.WriteLine(message);
            },
            (averageDuration) => Console.WriteLine($"Duración estimada: {averageDuration:F2} segundos"),
            (actualDuration) => Console.WriteLine($"Tiempo total de ejecución: {actualDuration:F2} segundos"),
            cts.Token);
        }
    }
}