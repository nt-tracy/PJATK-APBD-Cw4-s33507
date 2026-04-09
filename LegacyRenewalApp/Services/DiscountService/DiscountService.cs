using System;
using System.Collections.Generic;
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
    
    public CalculationResult GetDiscountResult(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        var discounts = new List<CalculationResult>
        {
            GetDiscountFromCustomerSegment(customer.Segment, plan.IsEducationEligible, baseAmount),
            GetDiscountFromYearsWithCompany(customer.YearsWithCompany, baseAmount),
            GetDiscountFromSeatCount(seatCount, baseAmount)
        };

        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
            discounts.Add(new CalculationResult(pointsToUse, $"loyalty points used: {pointsToUse}; "));
        }

        decimal totalAmount = 0;
        string totalNotes = string.Empty;

        foreach (var d in discounts)
        {
            totalAmount += d.Discount; 
            totalNotes += d.Note;  
        }

        return new CalculationResult(totalAmount, totalNotes);
    }
    
}
