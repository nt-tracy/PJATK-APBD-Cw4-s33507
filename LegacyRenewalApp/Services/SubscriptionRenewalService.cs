using System;
using LegacyRenewalApp.Exceptions;
using LegacyRenewalApp.Services.TaxProvider;
using LegacyRenewalApp.TaxProvider;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints
            )
        
        {
            //walidacja danych wejściowych
            if (customerId <= 0)
            {
                throw new NegativeCustomerIdException();
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new InvalidPlanCodeException();
            }

            if (seatCount <= 0)
            {
                throw new NegativeSeatCountException();
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new InvalidPaymentMethodException();
            }
            //koniec walidacji
            
            
            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            var planRepository = new SubscriptionPlanRepository();
            var plan = planRepository.GetByCode(normalizedPlanCode);

            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            var customerRepository = new CustomerRepository();
            var customer = customerRepository.GetById(customerId);


            if (!customer.IsActive)
            {
                throw new InactiveCustomerException();
            }
            
            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            IDiscountService discountService = new DiscountService();
            discountAmount += discountService.GetDiscountFromCustomerSegment(customer.Segment, customer.IsActive,  baseAmount).Discount
                            + discountService.GetDiscountFromSeatCount(seatCount, baseAmount).Discount
                            + discountService.GetDiscountFromYearsWithCompany(customer.YearsWithCompany, baseAmount).Discount;
            
            //uzywanie punktow
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }
            
            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            //kolejne inne
            decimal supportFee = 0m;
            if (includePremiumSupport)
            {
                if (normalizedPlanCode == "START")
                {
                    supportFee = 250m;
                }
                else if (normalizedPlanCode == "PRO")
                {
                    supportFee = 400m;
                }
                else if (normalizedPlanCode == "ENTERPRISE")
                {
                    supportFee = 700m;
                }

                notes += "premium support included; ";
            }

            
            //zaleznosci miedzy typami platnosci-do PriceCalcService
            decimal paymentFee = 0m;
            if (normalizedPaymentMethod == "CARD")
            {
                paymentFee = (subtotalAfterDiscount + supportFee) * 0.02m;
                notes += "card payment fee; ";
            }
            else if (normalizedPaymentMethod == "BANK_TRANSFER")
            {
                paymentFee = (subtotalAfterDiscount + supportFee) * 0.01m;
                notes += "bank transfer fee; ";
            }
            else if (normalizedPaymentMethod == "PAYPAL")
            {
                paymentFee = (subtotalAfterDiscount + supportFee) * 0.035m;
                notes += "paypal fee; ";
            }       
            else if (normalizedPaymentMethod == "INVOICE")
            {
                paymentFee = 0m;
                notes += "invoice payment; ";
            }
            else
            {
                throw new UnsupportedPaymentMethod();
            }
            
            //Adding Services
            ITaxProviderService providerService = new TaxProviderService();
            
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = providerService.CalculateTaxAmount(taxBase, customer.Country);
            var finalResult = providerService.FinalizeGrossAmount(taxBase, taxAmount);
            decimal finalAmount = finalResult.FinalAmount;
            
            
            

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = taxAmount,
                FinalAmount =finalAmount,
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            LegacyBillingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                LegacyBillingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
