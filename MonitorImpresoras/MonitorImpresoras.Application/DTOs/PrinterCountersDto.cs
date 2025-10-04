namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para contadores completos de impresora - QOPIQ
    /// </summary>
    public class PrinterCountersDto
    {
        public string PrinterName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Contadores de impresión
        public PrintCountersDto PrintCounters { get; set; } = new();

        // Contadores de scanner
        public ScanCountersDto ScanCounters { get; set; } = new();

        // Estado de consumibles
        public ConsumablesStatusDto Consumables { get; set; } = new();

        // Estado del fusor
        public FuserStatusDto Fuser { get; set; } = new();

        // Estado del tambor
        public DrumStatusDto Drum { get; set; } = new();
    }

    /// <summary>
    /// Contadores de impresión detallados
    /// </summary>
    public class PrintCountersDto
    {
        // Contadores generales
        public long TotalPages { get; set; }
        public long TotalPagesBlackWhite { get; set; }
        public long TotalPagesColor { get; set; }

        // Contadores por tamaño A4
        public long A4BlackWhite { get; set; }
        public long A4Color { get; set; }

        // Contadores por tamaño A3
        public long A3BlackWhite { get; set; }
        public long A3Color { get; set; }

        // Contadores por tamaño Letter
        public long LetterBlackWhite { get; set; }
        public long LetterColor { get; set; }

        // Contadores por tamaño Legal
        public long LegalBlackWhite { get; set; }
        public long LegalColor { get; set; }

        // Contadores de copias
        public long TotalCopies { get; set; }
        public long CopiesBlackWhite { get; set; }
        public long CopiesColor { get; set; }

        // Duplex
        public long DuplexPages { get; set; }
        public long SimplexPages { get; set; }
    }

    /// <summary>
    /// Contadores de scanner
    /// </summary>
    public class ScanCountersDto
    {
        public long TotalScans { get; set; }
        public long ScansBlackWhite { get; set; }
        public long ScansColor { get; set; }

        // Scans por tamaño
        public long ScansA4 { get; set; }
        public long ScansA3 { get; set; }
        public long ScansLetter { get; set; }
        public long ScansLegal { get; set; }

        // Scans por tipo
        public long ScansToPdf { get; set; }
        public long ScansToEmail { get; set; }
        public long ScansToFolder { get; set; }
        public long ScansToUsb { get; set; }

        // ADF vs Flatbed
        public long AdfScans { get; set; }
        public long FlatbedScans { get; set; }
    }

    /// <summary>
    /// Estado de consumibles
    /// </summary>
    public class ConsumablesStatusDto
    {
        // Toner levels (0-100%)
        public int? TonerBlackLevel { get; set; }
        public int? TonerCyanLevel { get; set; }
        public int? TonerMagentaLevel { get; set; }
        public int? TonerYellowLevel { get; set; }

        // Toner status
        public string TonerBlackStatus { get; set; } = string.Empty; // OK, Low, Empty, Unknown
        public string TonerCyanStatus { get; set; } = string.Empty;
        public string TonerMagentaStatus { get; set; } = string.Empty;
        public string TonerYellowStatus { get; set; } = string.Empty;

        // Ink levels (para inkjet)
        public int? InkBlackLevel { get; set; }
        public int? InkCyanLevel { get; set; }
        public int? InkMagentaLevel { get; set; }
        public int? InkYellowLevel { get; set; }

        // Waste toner box
        public int? WasteTonerLevel { get; set; }
        public string WasteTonerStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// Estado del fusor
    /// </summary>
    public class FuserStatusDto
    {
        public int? FuserLifeRemaining { get; set; } // Porcentaje 0-100%
        public long? FuserPageCount { get; set; }
        public long? FuserMaxPages { get; set; }
        public string FuserStatus { get; set; } = string.Empty; // OK, Warning, Replace, Unknown
        public bool NeedsReplacement { get; set; }
        public DateTime? LastReplacementDate { get; set; }
        public int? FuserTemperature { get; set; }
    }

    /// <summary>
    /// Estado del tambor/drum
    /// </summary>
    public class DrumStatusDto
    {
        public int? DrumLifeRemaining { get; set; } // Porcentaje 0-100%
        public long? DrumPageCount { get; set; }
        public long? DrumMaxPages { get; set; }
        public string DrumStatus { get; set; } = string.Empty; // OK, Warning, Replace, Unknown
        public bool NeedsReplacement { get; set; }
        public DateTime? LastReplacementDate { get; set; }

        // Para impresoras con múltiples tambores
        public int? DrumBlackLifeRemaining { get; set; }
        public int? DrumCyanLifeRemaining { get; set; }
        public int? DrumMagentaLifeRemaining { get; set; }
        public int? DrumYellowLifeRemaining { get; set; }
    }

    /// <summary>
    /// Datos completos de la impresora en una sola consulta
    /// </summary>
    public class CompletePrinterDataDto
    {
        public string PrinterName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Todos los contadores
        public PrinterCountersDto Counters { get; set; } = new();

        // Información adicional
        public string FirmwareVersion { get; set; } = string.Empty;
        public long? TotalMemory { get; set; }
        public long? AvailableMemory { get; set; }
        public string[] SupportedPaperSizes { get; set; } = Array.Empty<string>();
        public string[] InstalledOptions { get; set; } = Array.Empty<string>();

        // Alertas activas
        public List<string> ActiveAlerts { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
