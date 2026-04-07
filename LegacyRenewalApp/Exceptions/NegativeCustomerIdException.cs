using System;

namespace LegacyRenewalApp.Exceptions;

public class NegativeCustomerIdException()
    :Exception("Customer id must be positive");