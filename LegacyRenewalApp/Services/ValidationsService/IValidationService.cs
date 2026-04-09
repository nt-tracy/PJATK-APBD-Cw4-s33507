namespace LegacyRenewalApp;

public interface IValidationService
{
    void ValidateRenewalInputs(int customerId, string planCode, int seatCount, string paymentMethod);
    void ValidateBusinessRules(Customer customer);
}