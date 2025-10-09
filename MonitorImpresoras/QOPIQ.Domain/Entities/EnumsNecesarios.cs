namespace QOPIQ.Domain.Entities
{
    public enum BillingCycle
    {
        Monthly = 0,
        Yearly = 1
    }

    public enum PlanType
    {
        Free = 0,
        Basic = 1,
        Pro = 2,
        Enterprise = 3
    }

    public enum TenantStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2
    }

    public enum PaymentStatus
    {
        Unknown = 0,
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    public enum SubscriptionTier
    {
        Free = 0,
        Basic = 1,
        Pro = 2,
        Enterprise = 3
    }
}

