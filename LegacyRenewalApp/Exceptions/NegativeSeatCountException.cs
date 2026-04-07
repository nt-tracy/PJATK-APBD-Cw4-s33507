using System;

namespace LegacyRenewalApp.Exceptions;

public class NegativeSeatCountException() : Exception("Seat count must be positive");