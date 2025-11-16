using System.Xml;
using CsvHelper;

namespace Best_Worst_Of_Options;

public class Stock
{
    public string Ticker { get; }
    public Dictionary<DateTime, double> HistoricalPrices { get; set;}
    
    public Stock(string ticker)
    {
        Ticker = ticker;
        HistoricalPrices = [];
    }
}
