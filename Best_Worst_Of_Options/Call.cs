namespace Best_Worst_Of_Options;



/// <summary>
/// Option de type Call : payoff = max(SelectedUnderlying - Strike, 0)
/// where SelectedUnderlying = max(S_i) for BestOf or min(S_i) for WorstOf.
/// </summary>
public class Call : Option
{
    public Call(IEnumerable<string> underlyings, double strike, double maturity, PayoffType payoffType)
        : base(underlyings, strike, maturity, payoffType)
    {
    }

    public override double Payoff(double[] finalPrices)
    {
        double selected = SelectUnderlyingValue(finalPrices);
        return Math.Max(selected - Strike, 0.0);
    }
}

