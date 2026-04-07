using System;

namespace LegacyRenewalApp.Exceptions;

public class InvalidPlanCodeException()
    : Exception("Plan code is required");