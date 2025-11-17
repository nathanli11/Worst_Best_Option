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
        public Dictionary<string, Stock> Stocks { get; } = new();
        public Market(string csvPath)
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
            };

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();

            // Récupère tous les tickers sauf "Date"
            if (csv.HeaderRecord == null)
                throw new Exception("CSV sans en-tête");
            var tickers = csv.HeaderRecord.Skip(1).ToArray();

            // création des Stocks
            foreach (var t in tickers)
                Stocks[t] = new Stock(t);

            while (csv.Read())
            {
                var date = csv.GetField<DateTime>("Date");

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
    }
}
