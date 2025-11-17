using System;
using System.Collections.Generic;
using System.Globalization;   // ← obligatoire pour CultureInfo
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

using Best_Worst_Of_Options; // namespace de la classe Data

/*
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

data.LoadCsv("C:\\Users\\you\\Documents\\prices.csv");
data.ComputeCovarianceMatrix();

var mc = new MonteCarlo(data);
mc.Simulate(nbSimulations: 10000, nbDays: 252); // 1 an

// 2️⃣ Créer des options Worst-Of / Best-Of
Option callBest  = new Call(tickers, strike: 200.0, maturity: 1.0, payoffType: PayoffType.BestOf);
Option callWorst = new Call(tickers, strike: 200.0, maturity: 1.0, payoffType: PayoffType.WorstOf);
Option putBest   = new Put(tickers, strike: 200.0, maturity: 1.0, payoffType: PayoffType.BestOf);
Option putWorst  = new Put(tickers, strike: 200.0, maturity: 1.0, payoffType: PayoffType.WorstOf);

// 3️⃣ Prix Monte Carlo (avec taux sans risque 3%)
double r = 0.03;
Console.WriteLine($"Call Best-Of  : {mc.PriceOption(callBest, r):F2}");
Console.WriteLine($"Call Worst-Of : {mc.PriceOption(callWorst, r):F2}");
Console.WriteLine($"Put Best-Of   : {mc.PriceOption(putBest, r):F2}");
Console.WriteLine($"Put Worst-Of  : {mc.PriceOption(putWorst, r):F2}");
*/


// Charger les données du marché depuis un CSV dans un objet Market
string csvPath = "/Users/mba/Desktop/Cours/C_sharp/Worst_Best_Option/market_data.csv";
var market = new Market(csvPath);
market.ShowMarketData();

// Exemple d'accès à un stock
var apple = market["AAPL"];
Console.WriteLine($"Apple historical prices data points: {apple.HistoricalPrices.Count}");

// Exemple de création d'une option
var underlyings = new List<Stock> { market["AAPL"], market["MSFT"], market["GOOG"] };
var bestOfCallOption = new Call(underlyings, strike: 150.0, pricingDate: DateTime.Now, 
                                maturityDate: DateTime.Now.AddYears(1), payoffType: PayoffType.BestOf);
Console.WriteLine($"Created a Best-Of Call option with strike {bestOfCallOption.Strike}" + 
                    $" and time to maturity {bestOfCallOption.TimeToMaturity:F2} year(s).");