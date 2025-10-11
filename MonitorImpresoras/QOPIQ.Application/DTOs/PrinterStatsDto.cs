using System.Collections.Generic;

namespace QOPIQ.Application.DTOs
{
    public class PrinterStatsDto
    {
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        public int NeedsMaintenance { get; set; }
        public int LowOnSupplies { get; set; }
        public Dictionary<string, int> StatusCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ModelDistribution { get; set; } = new Dictionary<string, int>();
    }
}
