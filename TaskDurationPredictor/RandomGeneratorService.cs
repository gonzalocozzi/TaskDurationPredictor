namespace TaskDurationPredictor
{
    internal static class RandomGeneratorService
    {
        private static readonly Random _random = new();

        // Método para generar números aleatorios con distribución normal (Box-Muller)
        public static double NormalDistributionRandom()
        {
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();

            double standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            // Limitamos los valores extremos
            return Math.Max(Math.Min(standardNormal, 2), -2);
        }
    }
}