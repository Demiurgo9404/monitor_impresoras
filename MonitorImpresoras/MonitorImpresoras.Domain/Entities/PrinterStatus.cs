    /// <summary>
    /// Estado de una impresora obtenido vía SNMP
    /// </summary>
    public class PrinterStatusInfo
    {
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int? TonerLevel { get; set; }
        public int? PaperLevel { get; set; }
        public DateTime LastUpdate { get; set; }
    }
