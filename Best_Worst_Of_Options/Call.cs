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

    public override double Payoff(Dictionary<Stock, double> prices)
    {
        Stock underlying = SelectUnderlying(prices);
        double underlyingValue = prices[underlying];

        return Math.Max(underlyingValue - Strike, 0.0);
    }
}

