namespace Best_Worst_Of_Options;

public class Call : Option
{
    public Call(
        List<Stock> underlyings, double strike,
        DateTime pricingDate, DateTime maturityDate,
        PayoffType payoffType, double rate)
        : base(underlyings, strike, pricingDate, maturityDate, payoffType, rate)
    {
    }

    protected override double PayoffFromUnderlying(double S)
    {
        return Math.Max(S - Strike, 0.0);
    }
}