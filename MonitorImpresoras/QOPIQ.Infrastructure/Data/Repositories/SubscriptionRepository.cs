using Microsoft.EntityFrameworkCore;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QOPIQ.Infrastructure.Data.Repositories
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        private new readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Subscription?> GetActiveSubscriptionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .Where(s => s.UserId == userId && 
                          (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial) &&
                          (s.EndDate == null || s.EndDate > DateTime.UtcNow))
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        }

        public async Task AddInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            await _context.Set<Invoice>().AddAsync(invoice, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            _context.Set<Invoice>().Update(invoice);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(stripeSubscriptionId))
                throw new ArgumentException("Stripe subscription ID cannot be null or empty", nameof(stripeSubscriptionId));

            return await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, cancellationToken);
        }

        public async Task<bool> HasActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.UserId == userId && 
                             s.Status == SubscriptionStatus.Active &&
                             s.EndDate > DateTime.UtcNow, 
                         cancellationToken);
        }
    }
}
