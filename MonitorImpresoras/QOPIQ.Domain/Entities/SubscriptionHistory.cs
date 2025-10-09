using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    public class SubscriptionHistory : BaseEntity
    {
        public int SubscriptionId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string ChangeDescription { get; set; } = string.Empty;

        public virtual Subscription? Subscription { get; set; }
    }
}

