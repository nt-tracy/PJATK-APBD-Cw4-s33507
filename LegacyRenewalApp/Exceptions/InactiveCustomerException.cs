using System;

namespace LegacyRenewalApp.Exceptions;

public class InactiveCustomerException() : Exception("Inactive customers cannot renew subscriptions");