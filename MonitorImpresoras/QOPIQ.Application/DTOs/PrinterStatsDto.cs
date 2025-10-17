namespace QOPIQ.Application.DTOs
{
    public class PrinterStatsDto
    {
        public int TotalPrinters { get; set; }
        public int OnlinePrinters { get; set; }
        public int OfflinePrinters { get; set; }
        
        // Propiedades adicionales para el frontend
        public int ActiveCount => OnlinePrinters;
        public int WarningCount => 0; // TODO: Implementar lógica real
        public int InactiveCount => OfflinePrinters;
        public int Total => TotalPrinters;
    }
}
