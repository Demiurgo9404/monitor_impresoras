using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(ApplicationDbContext context, ILogger<SubscriptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(Guid userId)
        {
            try
            {
                return await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId.ToString() && s.Status == SubscriptionStatus.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscription for user {UserId}", userId);
                return null;
            }
        }

        public async Task<Subscription> CreateSubscriptionAsync(Guid userId, SubscriptionPlan plan)
        {
            try
            {
                var subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.ToString(),
                    PlanId = Guid.NewGuid(),
                    Status = SubscriptionStatus.Active,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created subscription {SubscriptionId} for user {UserId}", subscription.Id, userId);
                return subscription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Invoice> CreateInvoiceAsync(Guid subscriptionId, decimal amount)
        {
            try
            {
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscriptionId,
                    Amount = amount,
                    Currency = "USD",
                    Status = PaymentStatus.Pending,
                    DueDate = DateTime.UtcNow.AddDays(30),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created invoice {InvoiceId} for subscription {SubscriptionId}", invoice.Id, subscriptionId);
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task MarkInvoiceAsPaidAsync(Guid invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice != null)
                {
                    invoice.Status = PaymentStatus.Completed;
                    invoice.PaidAt = DateTime.UtcNow;
                    invoice.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Marked invoice {InvoiceId} as paid", invoiceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
                throw;
            }
        }
    }
}
