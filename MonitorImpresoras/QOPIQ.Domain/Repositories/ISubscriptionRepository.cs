using QOPIQ.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Repositories
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId);
        Task<IEnumerable<Invoice>> GetInvoicesForSubscriptionAsync(Guid subscriptionId);
        Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId);
        Task AddInvoiceAsync(Invoice invoice);
        void UpdateInvoice(Invoice invoice);
    }
}
