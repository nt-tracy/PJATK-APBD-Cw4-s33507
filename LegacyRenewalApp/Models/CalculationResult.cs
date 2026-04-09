namespace LegacyRenewalApp;

public class CalculationResult(decimal discount, string note)
{
    public decimal Discount { get; set; } = discount;
    public string Note { get; set; } = note;
}