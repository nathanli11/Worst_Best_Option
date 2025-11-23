using System;
using System.Collections.Generic;
using System.Linq;

namespace Best_Worst_Of_Options
{
    public static class MonteCarlo
    {
        // -----------------------
        // 1. Gaussian generator (Box-Muller)
        // -----------------------
        private static double NormalSample(Random rng)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        // -----------------------
        // 2. Compute empirical covariance (daily)
        // -----------------------
        private static double[,] ComputeCovarianceMatrix(List<double[]> returnsList)
        {
            int n = returnsList.Count;
            double[,] cov = new double[n, n];

            double[] means = returnsList.Select(r => r.Average()).ToArray();

            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double[] Ri = returnsList[i];
                    double[] Rj = returnsList[j];

                    int len = Math.Min(Ri.Length, Rj.Length);
                    double sum = 0.0;

                    for (int k = 0; k < len; k++)
                        sum += (Ri[k] - means[i]) * (Rj[k] - means[j]);

                    double covij = sum / (len - 1);
                    cov[i, j] = covij;
                    cov[j, i] = covij;
                }
            }

            return cov;
        }

        // -----------------------
        // 3. Covariance → correlation (daily)
        // -----------------------
        private static double[,] CovarianceToCorrelation(double[,] cov)
        {
            int n = cov.GetLength(0);
            double[,] corr = new double[n, n];

            double[] std = new double[n];
            for (int i = 0; i < n; i++)
                std[i] = Math.Sqrt(cov[i, i]);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double raw = cov[i, j] / (std[i] * std[j]);
                    corr[i, j] = Math.Clamp(raw, -1.0, 1.0);
                }
            }

            return corr;
        }

        // -----------------------
        // 4. Robust Cholesky decomposition
        // -----------------------
        private static double[,] Cholesky(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] L = new double[n, n];

            // small jitter for stability
            double eps = 1e-10;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = 0.0;
                    for (int k = 0; k < j; k++)
                        sum += L[i, k] * L[j, k];

                    if (i == j)
                    {
                        double v = A[i, i] - sum;
                        if (v <= 0) v = eps;     // jitter stabilizing
                        L[i, j] = Math.Sqrt(v);
                    }
                    else
                    {
                        L[i, j] = (A[i, j] - sum) / L[j, j];
                    }
                }
            }

            return L;
        }

        // --------------------------------------------------------------------
        // 5. FULL MONTE CARLO (DAILY PATHS) - robust & Best/Worst-of compatible
        // --------------------------------------------------------------------
        public static Dictionary<Stock, double[,]> MonteCarloSimulations(
            List<Stock> Underlyings,
            DateTime PricingDate,
            DateTime MaturityDate,
            int numPaths,
            double riskFreeRate)
        {
            int n = Underlyings.Count;
            int totalDays = (MaturityDate - PricingDate).Days;

            // Output: each stock → [path, time]
            var Paths = new Dictionary<Stock, double[,]>();
            foreach (var s in Underlyings)
                Paths[s] = new double[numPaths, totalDays + 1];

            // ---------------------------------------------
            // 1. Extract historical log-returns (daily)
            // ---------------------------------------------
            List<double[]> returnsList = new List<double[]>();
            double[] S0 = new double[n];

            for (int i = 0; i < n; i++)
            {
                var udl = Underlyings[i];

                var sorted = udl.HistoricalPrices
                                .OrderBy(h => h.Key)
                                .ToList();

                double[] prices = sorted.Select(h => h.Value).ToArray();
                S0[i] = prices.Last();

                // log returns
                double[] ret = new double[prices.Length - 1];
                for (int k = 1; k < prices.Length; k++)
                    ret[k - 1] = Math.Log(prices[k] / prices[k - 1]);

                returnsList.Add(ret);
            }

            // ---------------------------------------------
            // 2. Covariance / correlation / Cholesky
            // ---------------------------------------------
            double[,] cov = ComputeCovarianceMatrix(returnsList);
            double[,] corr = CovarianceToCorrelation(cov);
            double[,] chol = Cholesky(corr);

            // daily vols
            double[] sigmaDaily = new double[n];
            for (int i = 0; i < n; i++)
                sigmaDaily[i] = Math.Sqrt(cov[i, i]);

            // ---------------------------------------------
            // 3. Simulation parameters
            // ---------------------------------------------
            double dt = 1.0 / 252.0;
            double sqrtDt = Math.Sqrt(dt);

            // daily risk-neutral drift
            double[] driftDaily = new double[n];
            for (int i = 0; i < n; i++)
                driftDaily[i] = (riskFreeRate - 0.5 * sigmaDaily[i] * sigmaDaily[i]) * dt;

            Random rng = new Random();

            // ---------------------------------------------
            // 4. Monte Carlo simulation (full paths)
            // ---------------------------------------------
            for (int p = 0; p < numPaths; p++)
            {
                double[] spot = (double[])S0.Clone();

                // store S(0)
                for (int i = 0; i < n; i++)
                    Paths[Underlyings[i]][p, 0] = spot[i];

                for (int day = 1; day <= totalDays; day++)
                {
                    // Generate Z
                    double[] Z = new double[n];
                    for (int i = 0; i < n; i++)
                        Z[i] = NormalSample(rng);

                    // Correlate: Y = L * Z
                    double[] Y = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        double sum = 0.0;
                        for (int j = 0; j < n; j++)
                            sum += chol[i, j] * Z[j];
                        Y[i] = sum;
                    }

                    // Update spots
                    for (int i = 0; i < n; i++)
                    {
                        spot[i] *= Math.Exp(driftDaily[i] + sigmaDaily[i] * sqrtDt * Y[i]);
                        Paths[Underlyings[i]][p, day] = spot[i];
                    }
                }
            }

            return Paths;
        }
    }
}