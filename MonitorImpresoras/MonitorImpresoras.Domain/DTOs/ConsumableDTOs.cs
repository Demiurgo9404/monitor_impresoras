using System;
using System.Collections.Generic;

namespace MonitorImpresoras.Domain.DTOs
{
    public class ConsumableDTO
    {
        public Guid Id { get; set; }
        public Guid PrinterId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public int? MaxCapacity { get; set; }
        public int? CurrentLevel { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int? WarningLevel { get; set; }
        public int? CriticalLevel { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? RemainingPages { get; set; }
    }

    public class UpdateConsumableLevelDTO
    {
        public Guid ConsumableId { get; set; }
        public int? CurrentLevel { get; set; }
        public bool ResetCounter { get; set; }
    }

    public class ConsumableFilterDTO
    {
        public Guid? PrinterId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "low", "critical", "normal"
        public bool? NeedsReplacement { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ConsumableStatsDTO
    {
        public int TotalConsumables { get; set; } = 0;
        public int LowConsumables { get; set; } = 0;
        public int CriticalConsumables { get; set; } = 0;
        public int PrintersWithLowConsumables { get; set; } = 0;
        public Dictionary<string, int> ConsumablesByType { get; set; } = new();
        public List<ConsumableDTO> CriticalItems { get; set; } = new();
    }
}
