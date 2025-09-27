namespace MonitorImpresoras.Application.DTOs
{
    public class PrinterStatusInfo
    {
        public Guid PrinterId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastChecked { get; set; }
    }
}
