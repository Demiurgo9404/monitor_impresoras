using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.DTOs
{
    /// <summary>
    /// Solicitud para calcular costo de impresión
    /// </summary>
    public class CostEstimateRequestDTO
    {
        [Required]
        [Range(1, 10000)]
        public int Pages { get; set; }

        [Required]
        public bool IsColor { get; set; }

        [Required]
        public bool IsDuplex { get; set; }

        [Required]
        [StringLength(50)]
        public string PaperSize { get; set; } = "Carta";

        [StringLength(100)]
        public string PaperType { get; set; } = "Normal";

        [StringLength(100)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(0.1, 10)]
        public double CoveragePercentage { get; set; } = 1.0; // Porcentaje de cobertura de tinta/tóner
    }

    /// <summary>
    /// Respuesta con costo estimado detallado
    /// </summary>
    public class CostEstimateResponseDTO
    {
        public decimal TotalCost { get; set; }
        public decimal BaseCost { get; set; }
        public decimal ColorSurcharge { get; set; }
        public decimal DuplexDiscount { get; set; }
        public decimal PaperSurcharge { get; set; }
        public decimal DepartmentDiscount { get; set; }
        public decimal UserDiscount { get; set; }
        public List<string> AppliedPolicies { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public string Currency { get; set; } = "MXN";
    }

    /// <summary>
    /// Tarifas actuales del sistema
    /// </summary>
    public class CostRatesDTO
    {
        public decimal BaseRatePerPage { get; set; } = 0.50m; // Costo base por página B/N
        public decimal ColorSurchargePerPage { get; set; } = 0.30m; // Cargo adicional por color
        public decimal DuplexDiscountPercentage { get; set; } = 0.15m; // Descuento por dúplex
        public Dictionary<string, decimal> PaperSizeSurcharges { get; set; } = new()
        {
            ["Carta"] = 0.00m,
            ["Oficio"] = 0.05m,
            ["A4"] = 0.00m,
            ["A3"] = 0.20m,
            ["Legal"] = 0.10m
        };

        public Dictionary<string, decimal> PaperTypeSurcharges { get; set; } = new()
        {
            ["Normal"] = 0.00m,
            ["Brillante"] = 0.15m,
            ["Mate"] = 0.10m,
            ["Reciclado"] = -0.05m // Descuento por ecológico
        };

        public Dictionary<string, decimal> DepartmentDiscounts { get; set; } = new()
        {
            ["Ventas"] = 0.05m,
            ["Marketing"] = 0.10m,
            ["IT"] = 0.15m,
            ["RRHH"] = 0.20m,
            ["Finanzas"] = 0.25m
        };

        public Dictionary<string, decimal> UserDiscounts { get; set; } = new();
        public decimal CoverageAdjustmentFactor { get; set; } = 1.2m; // Factor para cobertura alta
        public string Currency { get; set; } = "MXN";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Solicitud para simular múltiples escenarios
    /// </summary>
    public class CostSimulationRequestDTO
    {
        public List<CostEstimateRequestDTO> Scenarios { get; set; } = new();
        public bool IncludeComparison { get; set; } = true;
        public string ComparisonMetric { get; set; } = "TotalCost"; // TotalCost, BaseCost, etc.
    }

    /// <summary>
    /// Resultado de simulación de costos
    /// </summary>
    public class CostSimulationResultDTO
    {
        public List<CostEstimateResponseDTO> Results { get; set; } = new();
        public ComparisonResultDTO Comparison { get; set; } = new();
        public decimal AverageCost { get; set; } = 0;
        public decimal MinimumCost { get; set; } = 0;
        public decimal MaximumCost { get; set; } = 0;
        public DateTime SimulatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Resultado de comparación entre escenarios
    /// </summary>
    public class ComparisonResultDTO
    {
        public string Metric { get; set; } = string.Empty;
        public List<ComparisonItemDTO> Items { get; set; } = new();
        public ComparisonItemDTO BestValue { get; set; } = new();
        public ComparisonItemDTO WorstValue { get; set; } = new();
    }

    /// <summary>
    /// Item individual en comparación
    /// </summary>
    public class ComparisonItemDTO
    {
        public int ScenarioIndex { get; set; }
        public string ScenarioDescription { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public bool IsBest { get; set; }
        public bool IsWorst { get; set; }
    }

    /// <summary>
    /// Configuración de políticas de costo
    /// </summary>
    public class CostPolicyDTO
    {
        public string PolicyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public List<string> Conditions { get; set; } = new();
        public decimal DiscountPercentage { get; set; }
        public decimal SurchargePercentage { get; set; }
        public List<string> ApplicableDepartments { get; set; } = new();
        public List<string> ApplicableUsers { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Historial de cálculos de costo
    /// </summary>
    public class CostCalculationHistoryDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Pages { get; set; }
        public bool IsColor { get; set; }
        public bool IsDuplex { get; set; }
        public string PaperSize { get; set; } = string.Empty;
        public string PaperType { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string JobId { get; set; } = string.Empty;
    }
}

