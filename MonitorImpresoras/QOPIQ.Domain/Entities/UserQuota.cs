using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    public class UserQuota : BaseEntity
    {
        public int UserId { get; set; }
        public int MaxPages { get; set; }
        public int UsedPages { get; set; }
        public DateTime ResetDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        public virtual UserEntity? User { get; set; }
        public virtual PrintingPolicy? Policy { get; set; }
    }
}

