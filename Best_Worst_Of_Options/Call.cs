namespace Best_Worst_Of_Options;

/// <summary>
/// Option de type Call : payoff = max(SelectedUnderlying - Strike, 0)
/// where SelectedUnderlying = max(S_i) for BestOf or min(S_i) for WorstOf.
/// </summary>
public class Call : Option
{
    public Call(List<Stock> underlyings, double strike, DateTime pricingDate, DateTime maturityDate, PayoffType payoffType, double rate)
        : base(underlyings, strike, pricingDate, maturityDate, payoffType, rate)
    {
    }

    public override double Payoff()
    {
        double underlyingValue = FinalPrices[SelectUnderlying()];
        Console.WriteLine($" Selected underlying for payoff: {SelectUnderlying().Ticker}");
        Console.WriteLine($" Selected underlying value for payoff: {underlyingValue:F2}");

        return Math.Max(underlyingValue - Strike, 0.0);
    }

    // Methode price (override de Option) qui calcule la date de maturité avec TimeToMaturity et PricingDate
    // et qui appelle MonteCarlo pour simuler les prix des stocks pendant la durée de vie de l'option
}

