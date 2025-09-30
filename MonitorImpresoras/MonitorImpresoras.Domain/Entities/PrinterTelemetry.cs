using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Domain.Entities
{
    public class PrinterTelemetry : BaseEntity
    {
        [Required]
        public int PrinterId { get; set; }
        
        [Required]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        // Métricas de estado
        public int PagesPrinted { get; set; }
        public int TonerLevel { get; set; }
        public int PaperLevel { get; set; }
        public int ErrorsCount { get; set; }
        public string Status { get; set; } = "Unknown";
        public decimal Temperature { get; set; }
        public bool IsOnline { get; set; }
        public string ErrorDetails { get; set; } = string.Empty;

        // Relaciones
        [ForeignKey("PrinterId")]
        public virtual Printer Printer { get; set; }

        // Filtro multi-tenant
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; }

        // Métodos auxiliares
        public bool IsCritical() => 
            TonerLevel < 10 || PaperLevel < 5 || Status == "Error" || Status == "PaperJam";
    }
}
