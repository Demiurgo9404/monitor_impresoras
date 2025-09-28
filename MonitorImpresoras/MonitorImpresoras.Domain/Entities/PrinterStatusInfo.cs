namespace MonitorImpresoras.Domain.Entities
{
    public class PrinterStatus
    {
        public int PrinterId { get; set; }
        public string PrinterName { get; set; } = string.Empty;
        public bool IsOffline { get; set; }
        public bool IsOutOfPaper { get; set; }
        public bool HasPaperJam { get; set; }
        public bool HasError { get; set; }
        public int TonerLevel { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
