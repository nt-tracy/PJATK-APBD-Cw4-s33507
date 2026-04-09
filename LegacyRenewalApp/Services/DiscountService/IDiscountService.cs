namespace LegacyRenewalApp;

public interface IDiscountService
{
    public DiscountResult GetDiscountFromYearsWithCompany(int yearsWithCompany, decimal baseAmount);
    public DiscountResult GetDiscountFromSeatCount(int seatCount, decimal baseAmount);
    public DiscountResult GetDiscountFromCustomerSegment(string segment, bool isEducationEligible ,decimal baseAmount);


}