using QOPIQ.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Domain.Interfaces.Repositories
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
        Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
        Task UpdateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    }
}
