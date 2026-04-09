using LegacyRenewalApp;

public interface ITaxProviderService
{
    public decimal GetTaxRate(string name);
    decimal CalculateTaxAmount(decimal taxBase, string customerCountry);
    (decimal FinalAmount, bool MinimumApplied) FinalizeGrossAmount(decimal taxBase, decimal taxAmount);
}