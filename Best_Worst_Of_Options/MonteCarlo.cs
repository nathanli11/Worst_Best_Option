namespace Best_Worst_Of_Options;

using System;

public class MonteCarlo
{
    private readonly Data data;

    public MonteCarlo(Data data)
    {
        this.data = data;
    }

    /// <summary>
    /// Simule des trajectoires de prix via un modèle de Brownien géométrique.
    /// </summary>
    /// <param name="nbSimulations">Nombre de simulations à générer.</param>
    /// <param name="nbDays">Nombre de jours à simuler.</param>
    /// <returns>Un tableau [nbSimulations, nbDays, nbTickers]</returns>
    public double[,,] Simulate(int nbSimulations, int nbDays)
    {
        int m = data.Tickers.Count;
        double[,,] simulations = new double[nbSimulations, nbDays, m];

        double[,] chol = CholeskyDecomposition(data.CovMatrix);
        Random rnd = new Random();

        for (int s = 0; s < nbSimulations; s++)
        {
            double[] currentPrices = data.Prices[^1]; // dernier prix

            for (int t = 0; t < nbDays; t++)
            {
                double[] z = new double[m];
                for (int i = 0; i < m; i++)
                    z[i] = NormalSample(rnd);

                // Appliquer Cholesky pour corréler les chocs
                double[] correlatedZ = new double[m];
                for (int i = 0; i < m; i++)
                {
                    correlatedZ[i] = 0;
                    for (int j = 0; j <= i; j++)
                        correlatedZ[i] += chol[i, j] * z[j];
                }

                // Générer les nouveaux prix
                for (int i = 0; i < m; i++)
                {
                    double mu = data.MeanReturns[i];
                    double sigma = Math.Sqrt(data.CovMatrix[i, i]);
                    double drift = mu - 0.5 * sigma * sigma;
                    double shock = sigma * correlatedZ[i];
                    currentPrices[i] *= Math.Exp(drift + shock);
                    simulations[s, t, i] = currentPrices[i];
                }
            }
        }

        return simulations;
    }

    // === Méthodes utilitaires ===

    private static double NormalSample(Random rnd)
    {
        // Génère un échantillon de N(0,1)
        double u1 = 1.0 - rnd.NextDouble();
        double u2 = 1.0 - rnd.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

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

    /// <summary>
    /// Calcule le prix d'une option par Monte Carlo.
    /// </summary>
    /// <param name="option">Option à évaluer (Call ou Put).</param>
    /// <param name="riskFreeRate">Taux sans risque annuel (ex : 0.03 pour 3%).</param>
    /// <returns>Prix théorique actuel de l’option.</returns>
    public double PriceOption(Option option, double riskFreeRate)
    {
        if (sims == null)
            throw new InvalidOperationException("Aucune simulation trouvée. Exécutez Simulate() avant.");

        int nbSim = sims.GetLength(0);
        int nbDays = sims.GetLength(1);
        int nbTickers = sims.GetLength(2);
        int lastDay = nbDays - 1;

        double sumPayoff = 0.0;

        for (int s = 0; s < nbSim; s++)
        {
            double[] finalPrices = new double[nbTickers];
            for (int i = 0; i < nbTickers; i++)
                finalPrices[i] = sims[s, lastDay, i];

            sumPayoff += option.Payoff(finalPrices);
        }

        double meanPayoff = sumPayoff / nbSim;
        double discounted = Math.Exp(-riskFreeRate * option.Maturity) * meanPayoff;

        return discounted;
    }

}

