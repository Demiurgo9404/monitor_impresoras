using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;
using QOPIQ.Domain.Interfaces;
using QOPIQ.Domain.Interfaces.Repositories;
using QOPIQ.Domain.Interfaces.Services;

namespace QOPIQ.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            IUnitOfWork unitOfWork)
        {
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Subscription> GetSubscriptionAsync(Guid id)
        {
            return await _subscriptionRepository.GetByIdAsync(id);
        }

        public async Task<Subscription> GetSubscriptionByUserAsync(Guid userId)
        {
            return await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
        }

        public async Task<Subscription> CreateSubscriptionAsync(Subscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            await _subscriptionRepository.AddAsync(subscription);
            await _unitOfWork.SaveChangesAsync();
            return subscription;
        }

        public async Task UpdateSubscriptionAsync(Subscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            _subscriptionRepository.Update(subscription);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelSubscriptionAsync(Guid id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription != null)
            {
                subscription.Status = SubscriptionStatus.Cancelled;
                subscription.EndDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> HasActiveSubscriptionAsync(Guid userId)
        {
            var subscription = await _subscriptionRepository.GetActiveSubscriptionByUserIdAsync(userId);
            return subscription != null && subscription.Status == SubscriptionStatus.Active;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesForSubscriptionAsync(Guid subscriptionId)
        {
            // Note: This method requires a repository method that's not in the interface
            // You'll need to add GetInvoicesBySubscriptionIdAsync to ISubscriptionRepository
            return new List<Invoice>();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(Guid invoiceId)
        {
            return await _subscriptionRepository.GetInvoiceByIdAsync(invoiceId);
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            await _subscriptionRepository.AddInvoiceAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
            return invoice;
        }

        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            await _subscriptionRepository.UpdateInvoiceAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
