using System;
using System.Collections.Generic;
using System.Globalization;   // ← obligatoire pour CultureInfo
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

using Best_Worst_Of_Options; // namespace de la classe Data
var tickers = new List<string> { "AAPL", "MSFT", "GOOG" };
DateTime startDate = DateTime.Now.AddYears(-3);
DateTime endDate = DateTime.Now;

var data = new Data();

Console.WriteLine("⏳ Récupération des données...");
await data.GetHistoricalDataAsync(tickers, startDate, endDate);

Console.WriteLine("\n📊 Résumé des données récupérées :");
foreach (var ticker in tickers)
{
    var prices = data.GetPrices(ticker);
    var lastDate = prices.Keys.Max();
    Console.WriteLine($"Ticker: {ticker} | Points: {prices.Count} | Dernier cours ({lastDate:yyyy-MM-dd}): {prices[lastDate]:F2}");
}

// Export CSV
string filePath = "historical_prices.csv";
data.ExportToCsv(filePath);

Console.WriteLine($"\n✅ Fichier CSV généré : {filePath}");