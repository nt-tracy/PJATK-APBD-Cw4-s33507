using LegacyRenewalApp.Exceptions;

namespace LegacyRenewalApp.Services.PriceCalculatorService;
public class PriceCalculatorService : IPriceCalculatorService
{
    public decimal CalculateBaseAmount(decimal monthlyPrice, int seatCount, decimal setupFee) 
        => (monthlyPrice * seatCount * 12m) + setupFee;

    public CalculationResult GetSupportFee(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport) return new CalculationResult(0, string.Empty);

        decimal fee = planCode switch
        {
            "START" => 250m,
            "PRO" => 400m,
            "ENTERPRISE" => 700m,
            _ => 0m
        };

        return new CalculationResult(fee, "premium support included; ");
    }

    public CalculationResult CalculatePaymentFee(decimal totalBase, string method)
    {
        return method switch
        {
            "CARD" => new CalculationResult(totalBase * 0.02m, "card payment fee; "),
            "BANK_TRANSFER" => new CalculationResult(totalBase * 0.01m, "bank transfer fee; "),
            "PAYPAL" => new CalculationResult(totalBase * 0.035m, "paypal fee; "),
            "INVOICE" => new CalculationResult(0, "invoice payment; "),
            _ => throw new UnsupportedPaymentMethod()
        };
    }
}