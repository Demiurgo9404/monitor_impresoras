using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para mostrar información de empresa
    /// </summary>
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string SubscriptionPlan { get; set; } = string.Empty;
        public int MaxPrinters { get; set; }
        public int MaxProjects { get; set; }
        public int MaxUsers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Estadísticas
        public int CurrentPrinters { get; set; }
        public int CurrentProjects { get; set; }
        public int CurrentUsers { get; set; }
    }

    /// <summary>
    /// DTO para crear nueva empresa
    /// </summary>
    public class CreateCompanyDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string TaxId { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Country { get; set; } = "Mexico";

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        [MaxLength(50)]
        public string SubscriptionPlan { get; set; } = "Basic";

        [Range(1, 1000)]
        public int MaxPrinters { get; set; } = 10;

        [Range(1, 100)]
        public int MaxProjects { get; set; } = 5;

        [Range(1, 500)]
        public int MaxUsers { get; set; } = 10;
    }

    /// <summary>
    /// DTO para actualizar empresa
    /// </summary>
    public class UpdateCompanyDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string TaxId { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Country { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        [MaxLength(50)]
        public string SubscriptionPlan { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int MaxPrinters { get; set; }

        [Range(1, 100)]
        public int MaxProjects { get; set; }

        [Range(1, 500)]
        public int MaxUsers { get; set; }
    }

    /// <summary>
    /// DTO para lista paginada de empresas
    /// </summary>
    public class CompanyListDto
    {
        public List<CompanyDto> Companies { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// DTO para estadísticas de empresa
    /// </summary>
    public class CompanyStatsDto
    {
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        
        // Contadores actuales
        public int TotalPrinters { get; set; }
        public int ActivePrinters { get; set; }
        public int InactivePrinters { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        // Límites
        public int MaxPrinters { get; set; }
        public int MaxProjects { get; set; }
        public int MaxUsers { get; set; }

        // Porcentajes de uso
        public double PrinterUsagePercentage => MaxPrinters > 0 ? (double)TotalPrinters / MaxPrinters * 100 : 0;
        public double ProjectUsagePercentage => MaxProjects > 0 ? (double)TotalProjects / MaxProjects * 100 : 0;
        public double UserUsagePercentage => MaxUsers > 0 ? (double)TotalUsers / MaxUsers * 100 : 0;

        // Estado del contrato
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public int? DaysUntilContractExpiry => ContractEndDate?.Subtract(DateTime.UtcNow).Days;
        public bool IsContractExpiringSoon => DaysUntilContractExpiry <= 30;
        public bool IsContractExpired => ContractEndDate <= DateTime.UtcNow;
    }
}
