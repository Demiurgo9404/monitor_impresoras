using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonitorImpresoras.Domain.Entities
{
    [Table("MaintenancePrediction")]
    public class MaintenancePrediction
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int PrinterId { get; set; }

        [Required]
        public DateTime PredictedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(50)]
        public string PredictionType { get; set; } = default!;

        [Required]
        public decimal Probability { get; set; }

        public DateTime? EstimatedDate { get; set; }

        public int? DaysUntilEvent { get; set; }

        public decimal Confidence { get; set; }

        [MaxLength(100)]
        public string ModelVersion { get; set; } = "1.0.0";

        [Column(TypeName = "jsonb")]
        public string? InputData { get; set; }

        [Column(TypeName = "jsonb")]
        public string? FeatureImportance { get; set; }

        [MaxLength(200)]
        public string? RecommendedAction { get; set; }

        public bool HumanReviewed { get; set; }

        [MaxLength(450)]
        public string? ReviewedBy { get; set; }

        [MaxLength(1000)]
        public string? ReviewComments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("PrinterId")]
        public virtual Printer? Printer { get; set; }
    }
}
