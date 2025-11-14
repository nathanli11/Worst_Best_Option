using CsvHelper;

namespace Best_Worst_Of_Options;

public class Stock
{
    public string Ticker { get; }
    public double Spot { get; set; }
    
    public Stock(string ticker, double spot)
    {
        Ticker = ticker;
        Spot = spot;
    }

}
