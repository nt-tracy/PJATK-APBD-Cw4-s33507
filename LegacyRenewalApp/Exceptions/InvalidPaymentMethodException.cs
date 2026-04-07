using System;

namespace LegacyRenewalApp.Exceptions;

public class InvalidPaymentMethodException() 
    : Exception("Payment method is required");