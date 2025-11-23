namespace Best_Worst_Of_Options
{
    public static class Grecs
    {
        /// <summary>
        /// Calcule les deltas d’une option multi-actifs
        /// via bump-and-revalue symétrique.
        /// </summary>
        public static double[] Delta(Option option, double h = 0.01, int numPaths = 5000)
        {
            int n = option.Underlyings.Count;
            double[] deltas = new double[n];

            // Lecture des spots initiaux à partir du dernier historique
            double[] S0 = option.Underlyings
                                .Select(s => s.HistoricalPrices.Values.Last())
                                .ToArray();

            // Pour chaque sous-jacent
            for (int i = 0; i < n; i++)
            {
                var stock = option.Underlyings[i];
                DateTime lastDate = stock.HistoricalPrices.Keys.Max();

                // Spot original
                double original = S0[i];

                // --------------------------
                //        BUMP UP
                // --------------------------
                stock.HistoricalPrices[lastDate] = original * (1.0 + h);
                double Vplus = option.Price(numPaths);

                // --------------------------
                //        BUMP DOWN
                // --------------------------
                stock.HistoricalPrices[lastDate] = original * (1.0 - h);
                double Vminus = option.Price(numPaths);

                // Restore
                stock.HistoricalPrices[lastDate] = original;

                // Delta
                deltas[i] = (Vplus - Vminus) / (2.0 * h * original);
            }

            return deltas;
        }
    }
}
