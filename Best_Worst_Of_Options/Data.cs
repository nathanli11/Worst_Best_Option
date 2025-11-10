using YahooFinanceApi;
using System.Globalization;
namespace Best_Worst_Of_Options;



public class Data
{
    private Dictionary<string, Dictionary<DateTime, double>> prices 
            = new Dictionary<string, Dictionary<DateTime, double>>();

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
}

