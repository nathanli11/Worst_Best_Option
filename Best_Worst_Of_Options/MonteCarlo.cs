using System;
using System.Collections.Generic;
using System.Linq;

namespace Best_Worst_Of_Options
{
    public static class MonteCarlo
    {
        // -----------------------
        //    1. Gaussian sample
        // -----------------------
        private static double NormalSample(Random rng)
        {
            // Box–Muller transform
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        // -----------------------
        // 2. Compute covariance (historical log-returns)
        // -----------------------
        private static double[,] ComputeCovarianceMatrix(List<double[]> returnsList)
        {
            int n = returnsList.Count;
            double[,] cov = new double[n, n];

            // Means
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
        // 3. Convert covariance → correlation
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
                    corr[i, j] = cov[i, j] / (std[i] * std[j]);
                }
            }

            return corr;
        }

        // -----------------------
        // 4. Cholesky decomposition
        // -----------------------
        private static double[,] Cholesky(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] L = new double[n, n];

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
                        if (v <= 0)
                            throw new Exception("Matrix not positive definite.");
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

        // -----------------------
        //      MAIN SIMULATOR
        // -----------------------
        public static Dictionary<Stock, double[]> MonteCarloSimulations(
            List<Stock> Underlyings,
            DateTime PricingDate,
            DateTime MaturityDate,
            int numPaths,
            double riskFreeRate)
        {
            int n = Underlyings.Count;
            Dictionary<Stock, double[]> FinalPrices =
                Underlyings.ToDictionary(s => s, s => new double[numPaths]);

            // -----------------------
            // 1. Extract historical log-returns
            // -----------------------
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

                double[] ret = new double[prices.Length - 1];
                for (int k = 1; k < prices.Length; k++)
                    ret[k - 1] = Math.Log(prices[k] / prices[k - 1]);

                returnsList.Add(ret);
            }

            // -----------------------
            // 2. Covariance -> vol -> correlation -> Cholesky
            // -----------------------
            double[,] cov = ComputeCovarianceMatrix(returnsList);
            double[] sigma = new double[n];

            for (int i = 0; i < n; i++)
            {
                double dailyVol = Math.Sqrt(cov[i, i]);
                sigma[i] = dailyVol * Math.Sqrt(252.0);   // annualized vol
            }

            double[,] corr = CovarianceToCorrelation(cov);
            double[,] chol = Cholesky(corr);

            // -----------------------
            // 3. Simulation parameters
            // -----------------------
            int totalDays = (MaturityDate - PricingDate).Days;
            double dt = 1.0 / 252.0;
            double sqrtDt = Math.Sqrt(dt);

            double[] drift = new double[n];
            for (int i = 0; i < n; i++)
                drift[i] = (riskFreeRate - 0.5 * sigma[i] * sigma[i]) * dt;

            Random rng = new Random();

            // -----------------------
            // 4. Monte Carlo simulation
            // -----------------------
            for (int p = 0; p < numPaths; p++)
            {
                double[] prices = (double[])S0.Clone();

                for (int day = 0; day < totalDays; day++)
                {
                    // Generate Z ~ N(0, I)
                    double[] Z = new double[n];
                    for (int i = 0; i < n; i++)
                        Z[i] = NormalSample(rng);

                    // Correlate: Y = chol * Z
                    double[] Y = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        double sum = 0.0;
                        for (int j = 0; j < n; j++)
                            sum += chol[i, j] * Z[j];
                        Y[i] = sum;
                    }

                    // Update prices (lognormal risk-neutral)
                    for (int i = 0; i < n; i++)
                    {
                        prices[i] *= Math.Exp(drift[i] + sigma[i] * sqrtDt * Y[i]);
                    }
                }

                // Store final prices
                for (int i = 0; i < n; i++)
                    FinalPrices[Underlyings[i]][p] = prices[i];
            }

            return FinalPrices;
        }
    }
}