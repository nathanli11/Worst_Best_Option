namespace Best_Worst_Of_Options;

public static class Grecs
{
    /// <summary>
    /// Calcule le vecteur Delta pour une option multi-sous-jacents.
    /// en utilisant la méthode "bump and revalue"
    /// </summary>
    public static double[] Delta(Option option, double h = 0.01, int numPaths = 5000)
    {
        // Récupération du nombre de sous jacent de l option
        int n = option.Underlyings.Count;

        // Creation du vecteur resultat des deltas
        double [] deltas = new double[n];

        // Recuperation des spots initiaux
        double [] S0 = option.Underlyings
                        .Select(s => s.HistoricalPrices.Values.Last())
                        .ToArray();
        
        // Pour chaque sous jacent
        for (int i=0; i < n; i++)
        {
            string ticker = option.Underlyings[i].Ticker;
            var stock = option.Underlyings[i];

            // Sauvegarde du spot price
            double original = S0[i];
            DateTime lastDate = stock.HistoricalPrices.Keys.Max();

            // Bump up
            stock.HistoricalPrices[lastDate] = original * (1+h);
            double VPlus = option.Price(numPaths);

            // Bump down
            stock.HistoricalPrices[lastDate] = original * (1 - h);
            double VMinus = option.Price(numPaths);

            // Restauration du spot price
            stock.HistoricalPrices[lastDate] = original;

            // Calcul du delta
            deltas[i] = (VPlus - VMinus) / (2.0 * h * original);
        }
        return deltas;
    }

}