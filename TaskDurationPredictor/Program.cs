using System;
using TaskDurationPredictor;

public class Program
{
    static void Main(string[] args)
    {
        string taskName = "TareaEjemplo";
        string historyFile = "taskHistory.json";
        TaskManager taskManager = new TaskManager(historyFile);

        if (taskManager.HasTaskHistory(taskName))
        {
            Console.WriteLine("Datos históricos encontrados. Estimando duración...");
            taskManager.SimulateTaskWithPrediction(taskName, ReportProgressWithPrediction);
        }
        else
        {
            Console.WriteLine("No hay datos históricos para esta tarea.");
            taskManager.SimulateTaskWithoutPrediction(taskName, ReportProgress);
        }
    }

    // Método para reportar el progreso y estimación restante
    static void ReportProgressWithPrediction(int progress, double estimatedRemaining)
    {
        Console.Write($"\rProgreso: {progress}% - Estimado restante: {estimatedRemaining:F2} segundos   ");
    }

    // Método para reportar solo el progreso
    static void ReportProgress(int progress)
    {
        Console.Write($"\rProgreso: {progress}%   ");
    }
}
