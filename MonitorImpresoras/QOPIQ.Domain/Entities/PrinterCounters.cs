using QOPIQ.Domain.Common;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.Entities
{
    public class PrinterCounters : BaseEntity
    {
        public int PrinterId { get; set; }
        public Printer Printer { get; set; } = null!;
        public DateTime RecordedAt { get; set; }
        public int TotalPrintedPages { get; set; }
        public int BlackAndWhitePages { get; set; }
        public int ColorPages { get; set; }
        public int CopiesMade { get; set; }
        public int ScannedPages { get; set; }
        public int FaxPages { get; set; }
        public double TonerLevel { get; set; }
        public int DrumUsage { get; set; }
        public int? MaintenanceCounter { get; set; }
        public int? PaperJams { get; set; }
        public int? ErrorCount { get; set; }

        // Método para determinar el estado general de la impresora
        public PrinterStatus GetOverallStatus()
        {
            if (TonerLevel <= 10)
                return PrinterStatus.Warning;
            if (TonerLevel <= 5)
                return PrinterStatus.Error;
            if (PaperJams > 0 || ErrorCount > 0)
                return PrinterStatus.Error;
                
            return PrinterStatus.Online;
        }

        // Método para obtener el nivel de tóner como porcentaje
        public double GetTonerPercentage()
        {
            return Math.Max(0, Math.Min(100, TonerLevel));
        }

        // Método para verificar si necesita mantenimiento
        public bool NeedsMaintenance()
        {
            return MaintenanceCounter.HasValue && MaintenanceCounter >= 10000;
        }

        // Método para calcular el uso total
        public int GetTotalUsage()
        {
            return TotalPrintedPages + ScannedPages + FaxPages;
        }
    }
}

