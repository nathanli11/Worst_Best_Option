using YahooFinanceApi;
using System.Globalization;
namespace Best_Worst_Of_Options;



public class Data
{
    private Dictionary<string, Dictionary<DateTime, double>> prices
            = new Dictionary<string, Dictionary<DateTime, double>>();

    public List<string> Tickers { get; private set; }
    public List<double[]> Prices { get; private set; }
    public double[] MeanReturns { get; private set; }
    public double[,] CovMatrix { get; private set; }

    public async Task ImportDataAsync(List<string> tickers, DateTime startDate, DateTime endDate)
    {
        foreach (var ticker in tickers)
        {
            try
            {
                var historicalData = await Yahoo.GetHistoricalAsync(ticker, startDate, endDate, Period.Daily);

                var closingPrices = historicalData.ToDictionary(data => data.DateTime.Date, data => (double)data.Close);

                prices[ticker] = closingPrices;

                Console.WriteLine($"Données récupérées pour {ticker} ({closingPrices.Count} points)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur pour {ticker} : {ex.Message}");
            }
        }
    }

    public Dictionary<DateTime, double> GetPrices(string ticker)
    {
        if (prices.ContainsKey(ticker))
            return prices[ticker];
        else
            throw new Exception($"Aucune donnée disponible pour {ticker}");
    }

    public void ExportToCsv(string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Date,Ticker,Close");

            foreach (var tickerEntry in prices)
            {
                string ticker = tickerEntry.Key;
                foreach (var dateEntry in tickerEntry.Value.OrderBy(d => d.Key))
                {
                    string line = $"{dateEntry.Key:yyyy-MM-dd},{ticker},{dateEntry.Value.ToString(CultureInfo.InvariantCulture)}";
                    writer.WriteLine(line);
                }
            }
        }

        Console.WriteLine($"Données exportées dans {filePath}");
    }

    /// <summary>
    /// Lit le fichier CSV et stocke les prix de clôture des tickers sélectionnés.
    /// </summary>
    public void LoadCsv(string csvPath)
    {
        var lines = File.ReadAllLines(csvPath);
        var headers = lines[0].Split(',');

        var indices = Tickers.Select(t => Array.IndexOf(headers, t)).ToArray();

        foreach (var line in lines.Skip(1))
        {
            var cols = line.Split(',');
            double[] row = new double[Tickers.Count];
            for (int i = 0; i < Tickers.Count; i++)
                row[i] = double.Parse(cols[indices[i]], CultureInfo.InvariantCulture);
            Prices.Add(row);
        }
    }

    /// <summary>
    /// Calcule les rendements moyens et la matrice de variance-covariance des tickers.
    /// </summary>
    public void ComputeCovarianceMatrix()
    {
        int n = Prices.Count - 1;
        int m = Tickers.Count;
        double[,] returns = new double[n, m];

        // Rendements log (ln(Pt / Pt-1))
        for (int i = 1; i < Prices.Count; i++)
        {
            for (int j = 0; j < m; j++)
            {
                returns[i - 1, j] = Math.Log(Prices[i][j] / Prices[i - 1][j]);
            }
        }

        // Moyennes des rendements
        MeanReturns = new double[m];
        for (int j = 0; j < m; j++)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
                sum += returns[i, j];
            MeanReturns[j] = sum / n;
        }

        // Matrice de covariance
        CovMatrix = new double[m, m];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < m; j++)
            {
                double sum = 0;
                for (int k = 0; k < n; k++)
                    sum += (returns[k, i] - MeanReturns[i]) * (returns[k, j] - MeanReturns[j]);
                CovMatrix[i, j] = sum / (n - 1);
            }
        }
    }

    public Data(List<string> tickers)
    {
        Tickers = tickers;
        Prices = new List<double[]>();
    }

}