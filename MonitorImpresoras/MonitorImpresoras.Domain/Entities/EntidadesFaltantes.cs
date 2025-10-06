using MonitorImpresoras.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Domain.Entities
{
    public class ScheduledReport : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string CronExpression { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? LastExecution { get; set; }
        public DateTime? NextExecution { get; set; }
    }

    public class ReportExecution : BaseEntity
    {
        public Guid ReportId { get; set; }
        public DateTime ExecutionDate { get; set; } = DateTime.UtcNow;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        
        public virtual Report Report { get; set; } = null!;
    }

    public class EmailTemplate : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string HtmlContent { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }

    public class ReportTemplate : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string TemplateContent { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string TemplateType { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}
