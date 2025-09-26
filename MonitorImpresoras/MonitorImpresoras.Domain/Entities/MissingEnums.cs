namespace MonitorImpresoras.Domain.Entities
{
    // Enums para planes y suscripciones
    public enum PlanType
    {
        Free,
        Basic,
        Professional,
        Enterprise
    }

    public enum SubscriptionTier
    {
        Free,
        Basic,
        Professional,
        Enterprise
    }

    public enum SubscriptionStatus
    {
        Active,
        Inactive,
        Suspended,
        Cancelled
    }

    public enum SubscriptionAction
    {
        Create,
        Renew,
        Cancel,
        Upgrade,
        Downgrade
    }

    public enum PaymentStatus
    {
        Unknown,
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public enum BillingCycle
    {
        Monthly,
        Yearly
    }

    // Enums para políticas
    public enum PolicyType
    {
        PrintingQuota,
        CostControl,
        Security,
        Custom
    }

    public enum EvaluationResult
    {
        Allowed,
        Denied,
        Warning
    }

    public enum AlertRuleType
    {
        Threshold,
        Anomaly,
        Pattern
    }

    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual
    }
}
