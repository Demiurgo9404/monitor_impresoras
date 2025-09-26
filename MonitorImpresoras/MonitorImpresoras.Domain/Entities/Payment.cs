using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Method { get; set; } = string.Empty;

        public virtual UserEntity? User { get; set; }
    }
}
