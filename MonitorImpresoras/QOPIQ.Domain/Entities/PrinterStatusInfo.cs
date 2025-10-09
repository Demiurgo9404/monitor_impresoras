namespace QOPIQ.Domain.Entities
{
    /// <summary>
    /// Estado de una impresora obtenido vía SNMP
    /// </summary>
    public class PrinterStatusInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int TotalPrintedPages { get; set; }
        public int BlackAndWhitePages { get; set; }
        public int ColorPages { get; set; }
        public double TonerLevel { get; set; }
        public bool HasPaperJam { get; set; }
        public bool IsOutOfPaper { get; set; }
        public bool IsOffline { get; set; }
        public bool HasError { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

