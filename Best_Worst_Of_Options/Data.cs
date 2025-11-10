using YahooFinanceApi;
using System.Globalization;
namespace Best_Worst_Of_Options;

public class Data
{
    
    // Ticker -> (Date -> Prix de clôture)
    private readonly Dictionary<string, Dictionary<DateTime, double>> prices
        = new Dictionary<string, Dictionary<DateTime, double>>();

    /// <summary>
    /// Récupère les prix historiques depuis Yahoo Finance.
    /// </summary>
    public async Task GetHistoricalDataAsync(List<string> tickers, DateTime startDate, DateTime endDate)
    {
        foreach (var ticker in tickers)
        {
            try
            {
                var history = await Yahoo.GetHistoricalAsync(ticker, startDate, endDate, Period.Daily);
                var tickerPrices = new Dictionary<DateTime, double>();

                foreach (var data in history)
                {
                    // conversion explicite decimal -> double
                    tickerPrices[data.DateTime] = (double)data.Close;
                }

                if (tickerPrices.Count > 0)
                {
                    prices[ticker] = tickerPrices;
                    Console.WriteLine($"✅ Données récupérées pour {ticker} ({tickerPrices.Count} points)");
                }
                else
                {
                    Console.WriteLine($"⚠️ Aucune donnée trouvée pour {ticker}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur pour {ticker} : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Retourne les prix d’un ticker donné.
    /// </summary>
    public Dictionary<DateTime, double> GetPrices(string ticker)
    {
        if (prices.ContainsKey(ticker))
            return prices[ticker];
        else
            throw new Exception($"Aucune donnée disponible pour {ticker}");
    }

    /// <summary>
    /// Exporte les données au format CSV.
    /// </summary>
    public void ExportToCsv(string filePath)
    {
        using var writer = new StreamWriter(filePath);
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

        Console.WriteLine($"📁 Données exportées dans {filePath}");
    }
}

