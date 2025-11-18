using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Best_Worst_Of_Options
{
    public class Market
    {
        // Creation d'un dictionnaire de Stocks
        public Dictionary<string, Stock> Stocks { get; } = new();
        // Constructeur
        public Market(string csvPath)
        {
            // Configuration de CsvHelper pour utiliser le ; comme délimiteur
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };

            // Lecture du fichier CSV
            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);
            csv.Read();
            csv.ReadHeader();

            // Récupère tous les tickers sauf "Date"
            if (csv.HeaderRecord == null)
                throw new Exception("CSV sans en-tête");

            // Récupération des tickers - nom de colonne
            var tickers = csv.HeaderRecord.Skip(1).ToArray();

            // création des Stocks
            foreach (var t in tickers)
                Stocks[t] = new Stock(t);

            while (csv.Read())
            {
                // Récupération de la date
                var date = csv.GetField<DateTime>("Date");

                // Récupération des prix pour chaque ticker
                for (int i = 0; i < tickers.Length; i++)
                {
                    double price = csv.GetField<double>(tickers[i]);
                    Stocks[tickers[i]].HistoricalPrices[date] = price;
                }
            }
        }

        // To be able to reference market["ticker"] easily
        public Stock this[string ticker] => Stocks[ticker];

        public void ShowMarketData()
        {
            foreach (var stock in Stocks)
            {
                if (stock.Value.HistoricalPrices.Count == 0)
                {
                    Console.WriteLine($"Ticker: {stock.Key}: <Aucune donnée>");
                    continue;
                }

                var last = stock.Value.HistoricalPrices.MaxBy(p => p.Key);

                Console.WriteLine(
                    $"Ticker: {stock.Key}, " +
                    $"Data Points: {stock.Value.HistoricalPrices.Count}, " +
                    $"Last Price: {last.Value:F2} (Date: {last.Key:yyyy-MM-dd})"
                );
            }
        }

        public Dictionary<string, List<Double[]>> MonteCarloSimulations(int days, int numPaths)
        {
            // Récupération de la liste des tickers
            var tickers = Stocks.Keys.ToArray();
            int n = tickers.Length;

            // Prix initiaux
            double []S0 = new double[n];
            // Liste qui contiendra les rendements historiques de l'actif i
            List<double[]> returnsList = new List<double[]>();

            // Calcul des rendements historiques
            for (int i = 0; i < n; i++)
            {
                var prices = Stocks[tickers[i]].HistoricalPrices.Values.ToArray();
                // Dernier prix
                S0[i]=prices.Last();

                // Rendements log
                var returns = new double[prices.Length - 1];
                for (int j = 1; j < prices.Length; j++)
                {  
                    returns[j - 1] = Math.Log(prices[j] / prices[j - 1]);
                }
                returnsList.Add(returns);
            }

            // Matrice de covariance et décomposition de Cholesky
            double[,] covMatrix = ComputeCovarianceMatrix(returnsList);
            double[,] chol = CholeskyDecomposition(covMatrix);

            // Simulation des trajectoires de prix
            var rand = new Random();
            // simulation[ticker] = liste des trajectoires
            var simulations = new Dictionary<string, List<Double[]>>();

            foreach (var t in tickers)
            {
                simulations[t] = new List<Double[]>();
            }

            // Simulation Monte Carlo
            for (int path=0; path < numPaths; path++)
            {
                // Copie des prix initiaux
                double[] prices = new double[n];
                Array.Copy(S0, prices, n);

                // Ajout d'un tableau vide pour chaque ticker
                foreach (var t in tickers)
                {
                    simulations[t].Add(new double[days+1]);
                }

                // Initial prices
                for (int i = 0; i < n; i++)
                {
                    simulations[tickers[i]][path][0] = S0[i];
                }

                // Simulation jour par jour
                for (int day=1; day <= days; day++)
                {
                    // Generation des variables aleatoires independantes normales
                    double[] Z = new double[n];
                    for (int i=0; i < n; i++)
                    {
                        Z[i] = NormalSample(rand);
                    }
                    // Application de Cholesky
                    double[] correlatedReturns = new double[n];
                    for (int i=0; i < n; i++)
                    {
                        correlatedReturns[i] = 0;
                        for (int j=0; j <= i; j++)
                        {
                            correlatedReturns[i] += chol[i, j] * Z[j];
                        }
                    }
                    // Mise a jour des prix (modèle Lognormal)
                    for (int i=0; i < n; i++)
                    {
                        double mu = returnsList[i].Average();
                        double sigma = Math.Sqrt(covMatrix[i, i]);
                        double drift = mu - 0.5 * sigma * sigma;
                        double shock = sigma * correlatedReturns[i];

                        // Prix simulés
                        prices[i] *= Math.Exp(drift + shock);
                        // Stockage du prix du jour simulé
                        simulations[tickers[i]][path][day] = prices[i];
                    }
                }
            }
            return simulations;
        }

        // Méthodes utilitaires
        private double NormalSample(Random rnd)
        {
            // Génère un échantillon de N(0,1)
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        private double[,] ComputeCovarianceMatrix(List<double[]> returnsList)
        {
            int n = returnsList.Count;
            double[,] covMatrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double meanI = returnsList[i].Average();
                    double meanJ = returnsList[j].Average();
                    double sum = 0.0;
                    int len = Math.Min(returnsList[i].Length, returnsList[j].Length);
                    for (int k = 0; k < len; k++)
                    {
                        sum += (returnsList[i][k] - meanI) * (returnsList[j][k] - meanJ);
                    }
                    covMatrix[i, j] = sum / (len - 1);
                    covMatrix[j, i] = covMatrix[i, j];
                }
            }
            return covMatrix;
        }

        private double[,] CholeskyDecomposition(double[,] matrix)
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
    }
}
