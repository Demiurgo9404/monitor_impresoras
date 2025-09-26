using System;

namespace MonitorImpresoras.Domain.Entities
{
    public class MonitoringStats
    {
        public int Id { get; set; }

        // Cantidad total de impresiones
        public int TotalPrints { get; set; }

        // Cantidad total de copias
        public int TotalCopies { get; set; }

        // Cantidad total de escaneos
        public int TotalScans { get; set; }

        // Nivel de tóner / consumibles
        public int TonerLevel { get; set; } // porcentaje 0-100

        // Fecha del último registro
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
