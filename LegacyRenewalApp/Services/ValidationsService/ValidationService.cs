using System;

namespace LegacyRenewalApp;
using LegacyRenewalApp.Exceptions;

public class ValidationService : IValidationService
{
    public void ValidateRenewalInputs(int customerId, string planCode, int seatCount, string paymentMethod)
    {
        if (customerId <= 0) throw new NegativeCustomerIdException();
        if (string.IsNullOrWhiteSpace(planCode)) throw new InvalidPlanCodeException();
        if (seatCount <= 0) throw new NegativeSeatCountException();
        if (string.IsNullOrWhiteSpace(paymentMethod)) throw new InvalidPaymentMethodException();
    }

    public void ValidateBusinessRules(Customer customer)
    {
        if (customer == null) throw new ArgumentNullException(nameof(customer));
        if (!customer.IsActive) throw new InactiveCustomerException();
    }
}