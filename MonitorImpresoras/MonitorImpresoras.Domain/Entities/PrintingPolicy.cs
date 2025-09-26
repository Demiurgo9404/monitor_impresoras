using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class PrintingPolicy : BaseEntity
    {
        public string PolicyName { get; set; } = string.Empty;
        public string Rules { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public virtual ICollection<PolicyEvaluationHistory>? Evaluations { get; set; }
        public virtual ICollection<UserQuota>? UserQuotas { get; set; }
    }
}
