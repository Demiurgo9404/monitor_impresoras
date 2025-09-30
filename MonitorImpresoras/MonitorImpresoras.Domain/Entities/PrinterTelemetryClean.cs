using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    [Table("PrinterTelemetryClean")]
    public class PrinterTelemetryClean
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int PrinterId { get; set; }

        [Required]
        public DateTime TimestampUtc { get; set; }

        public decimal? AvgPagesPrinted { get; set; }
        public decimal? AvgTonerLevel { get; set; }
        public decimal? AvgPaperLevel { get; set; }
        public int? TotalErrors { get; set; }

        [Required, MaxLength(20)]
        public string DominantStatus { get; set; } = default!;

        public decimal? AvgTemperature { get; set; }
        public decimal? AvgCpuUsage { get; set; }
        public decimal? AvgMemoryUsage { get; set; }
        public decimal? AvgJobsInQueue { get; set; }
        public long? AvgResponseTimeMs { get; set; }
        public int SampleCount { get; set; }
        public decimal DataQualityScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PrinterId")]
        public virtual Printer? Printer { get; set; }
    }
}
