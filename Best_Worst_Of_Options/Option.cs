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

    public double Price(int nbSimulations = 10000)
    {
        // Appeler MonteCarlo pour simuler les prix des stocks pendant la durée de vie de l'option
        // Appeler payoff pour calculer le payoff à l'échéance
        // Retourner la valeur actualisée du payoff
        var finalPrices = MonteCarlo.MonteCarloSimulations(Underlyings, PricingDate, MaturityDate, nbSimulations, riskFreeRate:0.02);

        double[] payoffs = new double[nbSimulations];
        for (int i = 0; i < nbSimulations; i++)
        {
            // Préparer le dictionnaire des prix finaux pour cette simulation
            Dictionary<Stock, double> prices = Underlyings.ToDictionary(s => s, s => finalPrices[s][i]);
            payoffs[i] = Payoff(prices);
        }
        // Moyenne des payoffs
        double averagePayoff = payoffs.Average();
        // Actualisation
        return averagePayoff * Math.Exp(-Rate * TimeToMaturity);
    }

    /// <summary>
    /// Calcule le payoff à l'échéance à partir des prix finaux des sous-jacents.
    /// Les classes dérivées doivent surcharger cette méthode.
    /// </summary>
    /// <returns>Valeur du payoff (>= 0).</returns>
    public abstract double Payoff(Dictionary<Stock, double> prices);

    /// <summary>
    /// Helper commun pour choisir la valeur sous-jacente selon BestOf/WorstOf.
    /// </summary>
    protected Stock SelectUnderlying(Dictionary<Stock, double> prices)
    {
        if (prices == null) throw new ArgumentNullException(nameof(prices));
        if (prices.Keys.Count != Underlyings.Count)
            throw new ArgumentException("Le nombre de prix fournis doit correspondre au nombre de sous-jacents.");

        return PayoffType == PayoffType.BestOf ?
            prices
            .OrderByDescending(fp => fp.Value)
            .First()
            .Key
            : 
            prices
            .OrderBy(kvp => kvp.Value)
            .First()
            .Key;
    }
}
