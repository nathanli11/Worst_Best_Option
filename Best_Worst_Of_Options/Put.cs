namespace Best_Worst_Of_Options;



/// <summary>
/// Option de type Put : payoff = max(Strike - SelectedUnderlying, 0)
/// where SelectedUnderlying = max(S_i) for BestOf or min(S_i) for WorstOf.
/// </summary>
public class Put : Option
{
    public Put(IEnumerable<string> underlyings, double strike, double maturity, PayoffType payoffType)
        : base(underlyings, strike, maturity, payoffType)
    {
    }

    public override double Payoff(double[] finalPrices)
    {
        double selected = SelectUnderlyingValue(finalPrices);
        return Math.Max(Strike - selected, 0.0);
    }
}
