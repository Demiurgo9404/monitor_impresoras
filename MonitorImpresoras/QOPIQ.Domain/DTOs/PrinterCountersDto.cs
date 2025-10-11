namespace QOPIQ.Domain.DTOs
{
    public class PrinterCountersDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public int BlackTonerLevel { get; set; }
        public int CyanTonerLevel { get; set; }
        public int MagentaTonerLevel { get; set; }
        public int YellowTonerLevel { get; set; }
        public int TotalPagesPrinted { get; set; }
    }
}
