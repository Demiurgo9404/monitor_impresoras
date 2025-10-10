using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Tipos de reportes disponibles en el sistema
    /// </summary>
    public enum ReportType
    {
        /// <summary>
        /// Reporte de uso de impresoras
        /// </summary>
        PrinterUsage,

        /// <summary>
        /// Reporte de costos de impresión
        /// </summary>
        CostAnalysis,

        /// <summary>
        /// Reporte de consumibles
        /// </summary>
        Consumable,

        /// <summary>
        /// Reporte de alertas
        /// </summary>
        Alerts,

        /// <summary>
        /// Reporte de políticas de impresión
        /// </summary>
        Policies,

        /// <summary>
        /// Reporte de usuarios
        /// </summary>
        Users,

        /// <summary>
        /// Reporte de trabajos de impresión
        /// </summary>
        PrintJobs,

        /// <summary>
        /// Reporte personalizado
        /// </summary>
        Custom
    }
}
