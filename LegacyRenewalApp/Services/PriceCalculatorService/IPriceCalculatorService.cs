namespace LegacyRenewalApp;

public interface IPriceCalculatorService
{
    decimal CalculateBaseAmount(decimal monthlyPrice, int seatCount, decimal setupFee);
    CalculationResult GetSupportFee(string planCode, bool includePremiumSupport);
    CalculationResult CalculatePaymentFee(decimal totalBeforeFee, string paymentMethod);
}