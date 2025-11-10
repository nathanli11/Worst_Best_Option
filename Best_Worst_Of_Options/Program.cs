using System;
using System.Collections.Generic;
using System.Globalization;   // ← obligatoire pour CultureInfo
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

using Best_Worst_Of_Options; // namespace de la classe Data

// Définir les tickers et la période
var tickers = new List<string> { "^GSPC" };
DateTime startDate = DateTime.Now.AddYears(-3);
DateTime endDate = DateTime.Now;

// Créer une instance de Data
var data = new Data();

// Récupérer les données depuis Yahoo Finance
Console.WriteLine("⏳ Récupération des données du S&P 500...");
await data.ImportDataAsync(tickers, startDate, endDate);

// Afficher un résumé des données
Console.WriteLine("\n📊 Résumé des données récupérées :");
foreach (var ticker in tickers)
{
    var prices = data.GetPrices(ticker);
    var lastDate = prices.Keys.Max();
    Console.WriteLine($"Ticker: {ticker} | Nombre de points: {prices.Count} | Dernier cours ({lastDate:yyyy-MM-dd}): {prices[lastDate]:F2}");
}

// Exporter dans un CSV
string filePath = "sp500_prices.csv";
data.ExportToCsv(filePath);

Console.WriteLine($"\n✅ Fichier CSV généré : {filePath}");
