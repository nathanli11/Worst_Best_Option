using System;
using System.Collections.Generic;
using Best_Worst_Of_Options;

// -----------------------------------------------------
// 1️⃣ Charger le marché
// -----------------------------------------------------

var market = new Market("market_data.csv");
market.ShowMarketData();

// Exemple d'accès à un stock
var apple = market["AAPL"];
Console.WriteLine($"Apple historical prices: {apple.HistoricalPrices.Count} points");

// -----------------------------------------------------
// 2️⃣ Sélection des sous-jacents
// -----------------------------------------------------

var underlyings = new List<Stock>
{
    market["AAPL"],
    market["MSFT"],
    market["GOOG"]
};

// -----------------------------------------------------
// 3️⃣ Création d'une option simple Best-Of
// -----------------------------------------------------

var bestOfCallOption = new Call(
    underlyings,
    strike: 500.0,
    pricingDate: DateTime.Now,
    maturityDate: DateTime.Now.AddYears(1),
    payoffType: PayoffType.BestOf,
    rate: 0.02
);

Console.WriteLine(
    $"Created Best-Of CALL | Strike: {bestOfCallOption.Strike}, " +
    $"Maturity: {bestOfCallOption.TimeToMaturity:F2} years"
);

// -----------------------------------------------------
// 4️⃣ Appel au Monte Carlo (nouvelle version FULL PATHS)
// -----------------------------------------------------

DateTime pricingDate = new DateTime(2025, 1, 1);
DateTime maturityDate = new DateTime(2026, 1, 1);

Console.WriteLine("\n⏳ Running Monte Carlo simulation...\n");

var paths = MonteCarlo.MonteCarloSimulations(
    underlyings,
    pricingDate,
    maturityDate,
    numPaths: 10000,
    riskFreeRate: 0.02
);

Console.WriteLine(
    $"Monte Carlo paths generated from {pricingDate:yyyy-MM-dd} to {maturityDate:yyyy-MM-dd}"
);

// -----------------------------------------------------
// 5️⃣ Exemple d'affichage : premier path, 10 premiers jours
// -----------------------------------------------------

foreach (var stock in underlyings)
{
    Console.WriteLine($"\nTicker: {stock.Ticker}");

    double[,] m = paths[stock];

    Console.WriteLine("First path (first 10 days):");

    for (int day = 0; day <= 10; day++)
    {
        Console.WriteLine($" Day {day}: {m[0, day]:F2}");
    }
}

// -----------------------------------------------------
// 6️⃣ Pricing de l’option Best-Of
// -----------------------------------------------------

double priceMC = bestOfCallOption.Price();
Console.WriteLine($"\n💰 Best-Of CALL option price (Monte Carlo): {priceMC:F2}\n");