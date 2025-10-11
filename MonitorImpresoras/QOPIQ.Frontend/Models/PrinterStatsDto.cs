namespace QOPIQ.Frontend.Models;

public class PrinterStatsDto
{
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int Total => ActiveCount + InactiveCount;
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public List<PrinterStatusHistory>? StatusHistory { get; set; }
}

public class PrinterStatusHistory
{
    public DateTime Date { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
}
