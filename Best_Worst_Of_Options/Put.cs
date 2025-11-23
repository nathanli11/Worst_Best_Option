namespace Best_Worst_Of_Options;

public class Put : Option
{
    public Put(
        List<Stock> underlyings, double strike,
        DateTime pricingDate, DateTime maturityDate,
        PayoffType payoffType, double rate)
        : base(underlyings, strike, pricingDate, maturityDate, payoffType, rate)
    {
    }

    protected override double PayoffFromUnderlying(double S)
    {
        return Math.Max(Strike - S, 0.0);
    }
}