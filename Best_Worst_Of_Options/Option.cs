namespace Best_Worst_Of_Options;

public enum PayoffType
{
    BestOf,
    WorstOf
}

public abstract class Option
{
    public double Strike { get; }
    public DateTime PricingDate { get; }
    public DateTime MaturityDate { get; }
    public double TimeToMaturity { get; }
    public double Rate { get; }
    public List<Stock> Underlyings { get; }
    public PayoffType PayoffType { get; }

    protected Option(
        List<Stock> underlyings,
        double strike,
        DateTime pricingDate,
        DateTime maturityDate,
        PayoffType payoffType,
        double rate)
    {
        if (underlyings == null || !underlyings.Any())
            throw new ArgumentException("Il faut au moins un sous-jacent.");

        Underlyings = underlyings;
        Strike = strike;
        PricingDate = pricingDate;
        MaturityDate = maturityDate;
        TimeToMaturity = (MaturityDate - PricingDate).TotalDays / 365.0;
        PayoffType = payoffType;
        Rate = rate;
    }

    // --------------------------------------------------------------------
    // 🔥 MÉTHODE DE PRICING MONTE CARLO — FULL PATHS — UTILISÉE PAR Program.cs
    // --------------------------------------------------------------------
    public double Price(int nbSimulations = 10000)
    {
        // 1️⃣ Simulation Monte Carlo complète (paths)
        var paths = MonteCarlo.MonteCarloSimulations(
            Underlyings,
            PricingDate,
            MaturityDate,
            nbSimulations,
            Rate
        );

        int lastDay = paths[Underlyings[0]].GetLength(1) - 1;

        double[] payoffs = new double[nbSimulations];

        // 2️⃣ Calcul des payoffs individuels
        for (int p = 0; p < nbSimulations; p++)
        {
            Dictionary<Stock, double> finalPrices = new();

            foreach (var s in Underlyings)
            {
                double[,] path = paths[s];
                double finalPrice = path[p, lastDay];
                finalPrices[s] = finalPrice;
            }

            payoffs[p] = Payoff(finalPrices);  // ← méthode générique
        }

        // 3️⃣ Moyenne des payoffs
        double meanPayoff = payoffs.Average();

        // 4️⃣ Actualisation
        return meanPayoff * Math.Exp(-Rate * TimeToMaturity);
    }

    // --------------------------------------------------------------------
    // 🔥 Payoff générique Best-Of / Worst-Of → délègue à Call/Put
    // --------------------------------------------------------------------
    public double Payoff(Dictionary<Stock, double> prices)
    {
        Stock selected = PayoffType == PayoffType.BestOf
            ? prices.OrderByDescending(p => p.Value).First().Key
            : prices.OrderBy(p => p.Value).First().Key;

        double S = prices[selected];

        return PayoffFromUnderlying(S);   // ← surcharge dans Call / Put
    }

    // --------------------------------------------------------------------
    // 🔥 Méthode abstraite pour Call et Put (surcharge obligatoire)
    // --------------------------------------------------------------------
    protected abstract double PayoffFromUnderlying(double S);
}