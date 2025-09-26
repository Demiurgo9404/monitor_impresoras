using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class TenantUser : BaseEntity
    {
        public string TenantName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }

        public virtual UserEntity? User { get; set; }
    }
}
