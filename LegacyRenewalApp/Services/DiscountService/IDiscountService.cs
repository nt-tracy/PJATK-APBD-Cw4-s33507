namespace LegacyRenewalApp;

public interface IDiscountService
{
    public CalculationResult GetDiscountFromYearsWithCompany(int yearsWithCompany, decimal baseAmount);
    public CalculationResult GetDiscountFromSeatCount(int seatCount, decimal baseAmount);
    public CalculationResult GetDiscountFromCustomerSegment(string segment, bool isEducationEligible ,decimal baseAmount);

    public CalculationResult GetDiscountResult(Customer customer, SubscriptionPlan plan, int seatCount,
        decimal baseAmount, bool useLoyaltyPoints);


}