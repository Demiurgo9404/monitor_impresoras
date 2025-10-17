using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Domain.Interfaces.Services
{
    public interface ISubscriptionService
    {
        Task<Subscription> GetSubscriptionAsync(Guid id);
        Task<Subscription> GetSubscriptionByUserAsync(Guid userId);
        Task<Subscription> CreateSubscriptionAsync(Subscription subscription);
        Task UpdateSubscriptionAsync(Subscription subscription);
        Task CancelSubscriptionAsync(Guid id);
        Task<bool> HasActiveSubscriptionAsync(Guid userId);
        Task<IEnumerable<Invoice>> GetInvoicesForSubscriptionAsync(Guid subscriptionId);
        Task<Invoice> GetInvoiceByIdAsync(Guid invoiceId);
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);
        Task UpdateInvoiceAsync(Invoice invoice);
    }
}
