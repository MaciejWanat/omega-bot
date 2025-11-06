namespace OmegaBot.Services
{
    public static class RandomNumbersProvider
    {
        private static readonly Random Random = new();

        public static int GetRandomNumberInRange(int min, int max) => Random.Next(min, max);

        public static double GetRandomNumberInRange(double min, double max) => Random.NextDouble() * (max - min) + min;

        public static T GetIdFromWeightedProbability<T>(IList<WeightedChanceParam<T>> parameters)
        {
            var numericValue = Random.NextDouble() * 100;

            foreach (var parameter in parameters)
            {
                numericValue -= parameter.Ratio;

                if (numericValue <= 0)
                    return parameter.ObjectToGet;
            }

            return parameters.Last().ObjectToGet;
        }

        public static bool GetPercentChanceSuccess(int percentChance) => Random.Next(100) < percentChance;
    }

    public class WeightedChanceParam<T>
    {
        public T ObjectToGet { get; init; }

        public double Ratio { get; init; }
    }
}
