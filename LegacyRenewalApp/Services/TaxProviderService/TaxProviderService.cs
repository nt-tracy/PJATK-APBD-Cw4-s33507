using System;
namespace LegacyRenewalApp;

public class TaxProviderService : ITaxProviderService
{
    public decimal GetTaxRate(string name) => name switch
    {
        "Poland" => 0.23m, 
        "Germany" => 0.19m,
        "CzechRepublic" => 0.21m,
        "Norway" => 0.25m,
        _ => 0.2m
    };
    
    public decimal CalculateTaxAmount(decimal taxBase, string country)
    {
        decimal rate = GetTaxRate(country);
        return Math.Round(taxBase * rate, 2, MidpointRounding.AwayFromZero);
    }

    public (decimal FinalAmount, bool MinimumApplied) FinalizeGrossAmount(decimal taxBase, decimal taxAmount)
    {
        decimal finalAmount = taxBase + taxAmount;
        bool minimumApplied = false;

        if (finalAmount < 500m)
        {
            finalAmount = 500m;
            minimumApplied = true;
        }

        return (Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero), minimumApplied);
    }

}