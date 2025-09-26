using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class UserEntity : BaseEntity
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Subscription>? Subscriptions { get; set; }
        public virtual ICollection<Payment>? Payments { get; set; }
        public virtual ICollection<TenantUser>? TenantUsers { get; set; }
        public virtual ICollection<UserQuota>? Quotas { get; set; }
    }
}
