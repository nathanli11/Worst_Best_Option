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
    public List<Stock> Underlyings { get; }
    public PayoffType PayoffType { get; }

    protected Option(List<Stock> underlyings, double strike, DateTime pricingDate, DateTime maturityDate, PayoffType payoffType)
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
    }

    /// <summary>
    /// Calcule le payoff à l'échéance à partir des prix finaux des sous-jacents.
    /// Les classes dérivées doivent surcharger cette méthode.
    /// </summary>
    /// <param name="finalPrices">Tableau de prix finaux, même ordre que Underlyings.</param>
    /// <returns>Valeur du payoff (>= 0).</returns>
    public abstract double Payoff(double[] finalPrices);

    /// <summary>
    /// Helper commun pour choisir la valeur sous-jacente selon BestOf/WorstOf.
    /// </summary>
    protected double SelectUnderlyingValue(double[] finalPrices)
    {
        if (finalPrices == null) throw new ArgumentNullException(nameof(finalPrices));
        if (finalPrices.Length != Underlyings.Count)
            throw new ArgumentException("Le nombre de prix fournis doit correspondre au nombre de sous-jacents.");

        return PayoffType == PayoffType.BestOf ? finalPrices.Max() : finalPrices.Min();
    }
}
