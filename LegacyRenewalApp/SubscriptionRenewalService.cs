using System;
using LegacyRenewalApp.Exceptions;
using LegacyRenewalApp.Services.DiscountService;
using LegacyRenewalApp.Services.PriceCalculatorService;
using LegacyRenewalApp.Services.TaxProviderService;

namespace LegacyRenewalApp.Services
{
    public class SubscriptionRenewalService
    {
        private readonly IDiscountService _discountService = new DiscountService.DiscountService();
        private readonly IPriceCalculatorService _priceCalculator = new PriceCalculatorService.PriceCalculatorService();
        private readonly ITaxProviderService _taxProvider = new TaxProviderService.TaxProviderService();

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            ValidateInput(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            var plan = new SubscriptionPlanRepository().GetByCode(normalizedPlanCode);
            var customer = new CustomerRepository().GetById(customerId);

            if (!customer.IsActive) throw new InactiveCustomerException();
            string notes = string.Empty;

            decimal baseAmount = _priceCalculator.CalculateBaseAmount(plan.MonthlyPricePerSeat, seatCount, plan.SetupFee);
            
            var discountResult = _discountService.GetDiscountResult(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
            decimal discountAmount = discountResult.Discount;
            notes += discountResult.Note;

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            var support = _priceCalculator.GetSupportFee(normalizedPlanCode, includePremiumSupport);
            notes += support.Note;

            var payment = _priceCalculator.CalculatePaymentFee(subtotalAfterDiscount + support.Discount, normalizedPaymentMethod);
            notes += payment.Note;

            decimal taxBase = subtotalAfterDiscount + support.Discount + payment.Discount;
            decimal taxAmount = _taxProvider.CalculateTaxAmount(taxBase, customer.Country);
            var finalResult = _taxProvider.FinalizeGrossAmount(taxBase, taxAmount);
            
            if (finalResult.MinimumApplied) notes += "minimum invoice amount applied; ";

            var invoice = CreateInvoiceObject(customerId, customer.FullName, normalizedPlanCode, normalizedPaymentMethod, seatCount, 
                                            baseAmount, discountAmount, support.Discount, payment.Discount, taxAmount, finalResult.FinalAmount, notes);

            LegacyBillingGateway.SaveInvoice(invoice);
            SendNotification(customer, invoice, normalizedPlanCode);

            return invoice;
        }

        private void ValidateInput(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0) throw new NegativeCustomerIdException();
            if (string.IsNullOrWhiteSpace(planCode)) throw new InvalidPlanCodeException();
            if (seatCount <= 0) throw new NegativeSeatCountException();
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new InvalidPaymentMethodException();
        }

        private void SendNotification(Customer customer, RenewalInvoice invoice, string planCode)
        {
            if (string.IsNullOrWhiteSpace(customer.Email)) return;

            string subject = "Subscription renewal invoice";
            string body = $"Hello {customer.FullName}, your renewal for plan {planCode} " +
                         $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

            LegacyBillingGateway.SendEmail(customer.Email, subject, body);
        }

        private RenewalInvoice CreateInvoiceObject(int customerId, string name, string plan, string method, int seats, 
            decimal baseAmt, decimal discAmt, decimal suppFee, decimal payFee, decimal taxAmt, decimal finalAmt, string notes)
        {
            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{plan}",
                CustomerName = name,
                PlanCode = plan,
                PaymentMethod = method,
                SeatCount = seats,
                BaseAmount = Math.Round(baseAmt, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discAmt, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(suppFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(payFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = taxAmt,
                FinalAmount = finalAmt,
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}