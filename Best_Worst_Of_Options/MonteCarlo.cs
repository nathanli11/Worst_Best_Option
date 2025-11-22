namespace Best_Worst_Of_Options;

using System;

public static class MonteCarlo
{
    // Tirage dans un échantillon normal standard
    private static double NormalSample(Random rng)
    {
        // Box-Muller transform
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    // Calculer la matrice de covariance des rendements historiques
    private static double[,] ComputeCovarianceMatrix(List<double[]> HistoricalReturns)
    {
        int n = HistoricalReturns.Count;
        double[,] covMatrix = new double[n, n];

        // Pré-calcul des moyennes
        double[] means = HistoricalReturns
            .Select(r => r.Average())
            .ToArray();

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double[] Ri = HistoricalReturns[i];
                double[] Rj = HistoricalReturns[j];

                int len = Math.Min(Ri.Length, Rj.Length);
                double sum = 0.0;

                for (int k = 0; k < len; k++)
                {
                    sum += (Ri[k] - means[i]) * (Rj[k] - means[j]);
                }

                double cov = sum / (len - 1);
                covMatrix[i, j] = cov;
                covMatrix[j, i] = cov;
            }
        }
        return covMatrix;
    }

    // Faire une décomposition de Cholesky
    private static double[,] CholeskyDecomposition(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double[,] L = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                double sum = 0;
                for (int k = 0; k < j; k++)
                    sum += L[i, k] * L[j, k];

                if (i == j)
                    L[i, j] = Math.Sqrt(matrix[i, i] - sum);
                else
                    L[i, j] = (matrix[i, j] - sum) / L[j, j];
            }
        }
        return L;
    }


    // Simuler les prix des sous-jacents à la date de maturité
    // Deux cas : la date de pricing est contenue dans les données historiques ou pas
    public static Dictionary<Stock, double> MonteCarloSimulations(
        List<Stock> Underlyings,
        DateTime PricingDate,
        DateTime MaturityDate,
        int numPaths)
    {
        int n = Underlyings.Count;
        // Prix initiaux
        Dictionary<Stock, double> S0 = new Dictionary<Stock, double> ();
        // Prix finaux moyens après simulation
        Dictionary<Stock, double> FinalPrices = Underlyings.ToDictionary(s => s, s => 0.0);

        // Definir la date de début de la simulation
        DateTime lastKnownDate = Underlyings.Min(s => s.HistoricalPrices.Keys.Max());
        bool pricingDateIsKnown = PricingDate <= lastKnownDate;
        DateTime startingDate = pricingDateIsKnown
            ? Underlyings
                .Select(udl => udl.HistoricalPrices.Keys
                    .Where(d => d <= PricingDate)
                    .Max()
                    )
                .Max()
            : lastKnownDate;

        // Défnir le nombre de jours à simuler
        int totalDays = (MaturityDate - startingDate).Days;
        
        // Rendements historiques des actifs
        List<double[]> HistoricalReturns = new List<double[]>();

        foreach (var udl in Underlyings)
        {
            // Récupérer le prix initial
            S0[udl] = udl.HistoricalPrices[startingDate];

            // Calculer les rendements historiques
            var prices = udl.HistoricalPrices
                .Where(h => h.Key <= startingDate)
                .OrderBy(h => h.Key)
                .Select(h => h.Value)
                .ToArray();

            double[] returns = new double[prices.Length - 1];
            for (int i = 1; i < prices.Length; i++)
            {
                returns[i - 1] = Math.Log(prices[i] / prices[i - 1]);
            }
            HistoricalReturns.Add(returns);
        }

        // Matrice de covariance et décomposition de Cholesky
        double[,] covMatrix = ComputeCovarianceMatrix(HistoricalReturns);
        double[,] chol = CholeskyDecomposition(covMatrix);

        // Calcul des drifts (moyenne historique des rendements)
        double[] means = HistoricalReturns.Select(r => r.Average()).ToArray();

        
        // accumulateurs pour la moyenne des prix finaux
        double[] accumulated = new double[n];

        Random rng = new Random();
        for (int path = 0; path < numPaths; path++)
        {
            // copie des prix courants
            double[] prices = Underlyings.Select(u => S0[u]).ToArray();

            for (int day = 0; day < totalDays; day++)
            {
                // Tirage multi-dimensionnel normal corrélé via Cholesky
                double[] Z = new double[n];
                for (int i = 0; i < n; i++)
                    Z[i] = NormalSample(rng);

                double[] correlatedZ = new double[n];
                for (int i = 0; i < n; i++)
                {
                    correlatedZ[i] = 0.0;
                    for (int j = 0; j < n; j++)
                        correlatedZ[i] += chol[i, j] * Z[j];
                }

                // Update des prix journaliers
                for (int i = 0; i < n; i++)
                {
                    prices[i] *= Math.Exp(means[i] + correlatedZ[i]);
                }
            }

            // Ajout au cumul pour la moyenne
            for (int i = 0; i < n; i++)
                accumulated[i] += prices[i];
        }

        // Moyennage des prix simulés
        for (int i = 0; i < n; i++)
            FinalPrices[Underlyings[i]] = accumulated[i] / numPaths;

        return FinalPrices;
    }
}

