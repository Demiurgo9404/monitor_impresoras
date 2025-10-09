using QOPIQ.Domain.Common;

namespace QOPIQ.Domain.Entities
{
    public class PolicyEvaluationHistory : BaseEntity
    {
        public int PolicyId { get; set; }
        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
        public bool Passed { get; set; }
        public string Reason { get; set; } = string.Empty;

        public virtual PrintingPolicy? Policy { get; set; }
    }
}

