namespace LegacyRenewalApp;

public class BillingService : IBillingService {
    public void Save(RenewalInvoice invoice) => LegacyBillingGateway.SaveInvoice(invoice);
    public void NotifyCustomer(Customer customer, RenewalInvoice invoice, string planCode) {
        
        if (string.IsNullOrWhiteSpace(customer.Email)) return;
        
        string subject = "Subscription renewal invoice";
        string body = $"Hello {customer.FullName}, your renewal for plan {planCode} has been prepared. Final amount: {invoice.FinalAmount:F2}.";
        
        LegacyBillingGateway.SendEmail(customer.Email, subject, body);
    }
}