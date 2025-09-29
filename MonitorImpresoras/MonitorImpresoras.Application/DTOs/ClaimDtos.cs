namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para representar claims de usuario
    /// </summary>
    public class UserClaimDto
    {
        public int Id { get; set; }
        public string ClaimType { get; set; } = default!;
        public string ClaimValue { get; set; } = default!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string? CreatedByUserId { get; set; }
    }

    /// <summary>
    /// DTO para definición de claims disponibles
    /// </summary>
    public class ClaimDefinitionDto
    {
        public string ClaimType { get; set; } = default!;
        public string ClaimValue { get; set; } = default!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool RequiresApproval { get; set; } = false;
        public int? MaxUsers { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
    }

    /// <summary>
    /// DTO para asignar claim a usuario
    /// </summary>
    public class AssignClaimDto
    {
        public string ClaimType { get; set; } = default!;
        public string ClaimValue { get; set; } = default!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
    }

    /// <summary>
    /// DTO para filtros de claims
    /// </summary>
    public class ClaimSearchDto
    {
        public string? Category { get; set; }
        public bool? IsActive { get; set; }
        public string? SearchTerm { get; set; }
    }
}
