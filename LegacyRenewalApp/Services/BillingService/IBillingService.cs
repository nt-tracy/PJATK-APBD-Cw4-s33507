namespace LegacyRenewalApp;

public interface IBillingService {
    void Save(RenewalInvoice invoice);
    void NotifyCustomer(Customer customer, RenewalInvoice invoice, string planCode);
}