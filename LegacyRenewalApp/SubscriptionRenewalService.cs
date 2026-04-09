using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly IDiscountService _discountService;
        private readonly IPriceCalculatorService _priceCalculator;
        private readonly ITaxProviderService _taxProvider;
        private readonly IValidationService _validationService;
        private readonly IBillingService _billingService;
        
        public SubscriptionRenewalService() : this(
            new DiscountService(), 
            new PriceCalculatorService(), 
            new TaxProviderService(), 
            new ValidationService(),
            new BillingService()) { }
        
        public SubscriptionRenewalService(
            IDiscountService discountService,
            IPriceCalculatorService priceCalculator,
            ITaxProviderService taxProvider,
            IValidationService validationService,
            IBillingService billingService)
        {
            _discountService = discountService;
            _priceCalculator = priceCalculator;
            _taxProvider = taxProvider;
            _validationService = validationService;
            _billingService = billingService;
        }
        
        

        public RenewalInvoice CreateRenewalInvoice(int customerId, string planCode, int seatCount, string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints)
        {
            _validationService.ValidateRenewalInputs(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            var plan = new SubscriptionPlanRepository().GetByCode(normalizedPlanCode);
            var customer = new CustomerRepository().GetById(customerId);
            
            _validationService.ValidateBusinessRules(customer);

            decimal baseAmount = _priceCalculator.CalculateBaseAmount(plan.MonthlyPricePerSeat, seatCount, plan.SetupFee);
            var discount = _discountService.GetDiscountResult(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
            
            decimal subtotal = Math.Max(300m, baseAmount - discount.Discount);
            string notes = discount.Note;

            if (subtotal <= 300m && baseAmount - discount.Discount < 300m)
            {
                notes += "minimum discounted subtotal applied; ";
            }

            var support = _priceCalculator.GetSupportFee(normalizedPlanCode, includePremiumSupport);
            var payment = _priceCalculator.CalculatePaymentFee(subtotal + support.Discount, paymentMethod.Trim().ToUpperInvariant());

            decimal taxBase = subtotal + support.Discount + payment.Discount;
            decimal taxAmount = _taxProvider.CalculateTaxAmount(taxBase, customer.Country);
            var finalResult = _taxProvider.FinalizeGrossAmount(taxBase, taxAmount);

            notes += support.Note + payment.Note;
            if (finalResult.MinimumApplied)
            {
                notes += "minimum invoice amount applied; ";
            }

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = paymentMethod.Trim().ToUpperInvariant(),
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discount.Discount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(support.Discount, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(payment.Discount, 2, MidpointRounding.AwayFromZero),
                TaxAmount = taxAmount,
                FinalAmount = finalResult.FinalAmount,
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            _billingService.Save(invoice);
            _billingService.NotifyCustomer(customer, invoice, normalizedPlanCode);

            return invoice;
        }
    }
}