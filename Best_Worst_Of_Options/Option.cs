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
    public double TimeToMaturity { get; } // en années
    public double Rate { get; } // taux sans risque
    public List<Stock> Underlyings { get; }
    public PayoffType PayoffType { get; }
    public Dictionary<Stock, double> FinalPrices { get; set; } = new();

    protected Option(List<Stock> underlyings, double strike, DateTime pricingDate, DateTime maturityDate, PayoffType payoffType, double rate)
    {
        if (underlyings == null) throw new ArgumentNullException(nameof(underlyings));
        var list = underlyings.ToList();
        if (!list.Any()) throw new ArgumentException("Il faut au moins un sous-jacent.", nameof(underlyings));

        Underlyings = underlyings;
        Strike = strike;
        PricingDate = pricingDate;
        MaturityDate = maturityDate;
        TimeToMaturity = (MaturityDate - PricingDate).TotalDays / 365.0;
        PayoffType = payoffType;
        Rate = rate;
    }

    public double Price()
    {
        // Appeler MonteCarlo pour simuler les prix des stocks pendant la durée de vie de l'option
        // Appeler payoff pour calculer le payoff à l'échéance
        // Retourner la valeur actualisée du payoff
        FinalPrices = MonteCarlo.MonteCarloSimulations(Underlyings, PricingDate, MaturityDate, numPaths: 10000);
        Console.WriteLine(" Final prices at maturity:");
        foreach (var stock in FinalPrices.Keys)
        {            Console.WriteLine($"  Ticker: {stock.Ticker}, Simulated Price: {FinalPrices[stock]:F2}");        }
        return Payoff() * Math.Exp(-Rate * TimeToMaturity);
    }

    /// <summary>
    /// Calcule le payoff à l'échéance à partir des prix finaux des sous-jacents.
    /// Les classes dérivées doivent surcharger cette méthode.
    /// </summary>
    /// <returns>Valeur du payoff (>= 0).</returns>
    public abstract double Payoff();

    /// <summary>
    /// Helper commun pour choisir la valeur sous-jacente selon BestOf/WorstOf.
    /// </summary>
    protected Stock SelectUnderlying()
    {
        if (FinalPrices == null) throw new ArgumentNullException(nameof(FinalPrices));
        if (FinalPrices.Keys.Count != Underlyings.Count)
            throw new ArgumentException("Le nombre de prix fournis doit correspondre au nombre de sous-jacents.");

        return PayoffType == PayoffType.BestOf ?
            FinalPrices
            .OrderByDescending(fp => fp.Value)
            .First()
            .Key
            : 
            FinalPrices
            .OrderBy(kvp => kvp.Value)
            .First()
            .Key;
    }
}
