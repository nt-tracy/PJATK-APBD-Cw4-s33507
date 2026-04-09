namespace LegacyRenewalApp;

public class DiscountResult(decimal discount, string note)
{
    public decimal Discount { get; set; } = discount;
    public string Note { get; set; } = note;
}