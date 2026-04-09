using System;

namespace LegacyRenewalApp;

public class DiscountService : IDiscountService
{
    public CalculationResult GetDiscountFromYearsWithCompany(int yearsWithCompany, decimal baseAmount)
        => yearsWithCompany switch
        {
            >= 5 => new CalculationResult(baseAmount * 0.07m, "long-term loyalty discount; "),
            >= 2 => new CalculationResult(baseAmount * 0.03m, "basic loyalty discount; "),
            _ => new CalculationResult(0, string.Empty)
        };


    public CalculationResult GetDiscountFromSeatCount(int seatCount, decimal baseAmount)
        => seatCount switch
        {
            >= 50 => new CalculationResult(baseAmount * 0.12m, "large team discount; "),
            >= 20 => new CalculationResult(baseAmount * 0.08m, "medium team discount; "),
            >= 10 => new CalculationResult(baseAmount * 0.04m, "small team discount; "),
            _ => new CalculationResult(0, string.Empty)
        };

    public CalculationResult GetDiscountFromCustomerSegment(string segment, bool isEducationEligible, decimal baseAmount)
    {
        if (isEducationEligible)
        {
            return segment switch
            {
                "Silver" => new CalculationResult(baseAmount * 0.05m, "silver discount; "),
                "Gold" => new CalculationResult(baseAmount * 0.10m, "gold discount; "),
                "Platinum" => new CalculationResult(baseAmount * 0.15m, "platinum discount; "),
                "Education" => new CalculationResult(baseAmount * 0.20m, "education discount; "),
                _ => new CalculationResult(0, string.Empty)
            };

        }

        return new CalculationResult( 0.0m, String.Empty);
    }
}
