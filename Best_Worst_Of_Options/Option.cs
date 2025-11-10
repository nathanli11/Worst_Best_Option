namespace Best_Worst_Of_Options;


public enum PayoffType
{
    BestOf,
    WorstOf
}

public abstract class Option
{
    public double Strike { get; }
    public double Maturity { get; } // en années
    public IReadOnlyList<string> Underlyings { get; }
    public PayoffType PayoffType { get; }

    protected Option(IEnumerable<string> underlyings, double strike, double maturity, PayoffType payoffType)
    {
        if (underlyings == null) throw new ArgumentNullException(nameof(underlyings));
        var list = underlyings.ToList();
        if (!list.Any()) throw new ArgumentException("Il faut au moins un sous-jacent.", nameof(underlyings));

        Underlyings = list.AsReadOnly();
        Strike = strike;
        Maturity = maturity;
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
