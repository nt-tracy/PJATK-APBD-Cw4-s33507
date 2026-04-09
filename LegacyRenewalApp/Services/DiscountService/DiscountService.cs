using System;

namespace LegacyRenewalApp;

public class DiscountService : IDiscountService
{
    public DiscountResult GetDiscountFromYearsWithCompany(int yearsWithCompany, decimal baseAmount)
        => yearsWithCompany switch
        {
            >= 5 => new DiscountResult(baseAmount * 0.07m, "long-term loyalty discount; "),
            >= 2 => new DiscountResult(baseAmount * 0.03m, "basic loyalty discount; "),
            _ => new DiscountResult(0, string.Empty)
        };


    public DiscountResult GetDiscountFromSeatCount(int seatCount, decimal baseAmount)
        => seatCount switch
        {
            >= 50 => new DiscountResult(baseAmount * 0.12m, "large team discount; "),
            >= 20 => new DiscountResult(baseAmount * 0.08m, "medium team discount; "),
            >= 10 => new DiscountResult(baseAmount * 0.04m, "small team discount; "),
            _ => new DiscountResult(0, string.Empty)
        };

    public DiscountResult GetDiscountFromCustomerSegment(string segment, bool isEducationEligible, decimal baseAmount)
    {
        if (isEducationEligible)
        {
            return segment switch
            {
                "Silver" => new DiscountResult(baseAmount * 0.05m, "silver discount; "),
                "Gold" => new DiscountResult(baseAmount * 0.10m, "gold discount; "),
                "Platinum" => new DiscountResult(baseAmount * 0.15m, "platinum discount; "),
                "Education" => new DiscountResult(baseAmount * 0.20m, "education discount; "),
                _ => new DiscountResult(0, string.Empty)
            };

        }

        return new DiscountResult( 0.0m, String.Empty);
    }
}
