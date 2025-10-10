using System.ComponentModel;

namespace QOPIQ.Domain.Enums
{
    /// <summary>
    /// Represents the format of a report
    /// </summary>
    public enum ReportFormat
    {
        /// <summary>
        /// Portable Document Format (PDF)
        /// </summary>
        [Description("PDF")]
        Pdf = 0,

        /// <summary>
        /// Microsoft Excel format (XLSX)
        /// </summary>
        [Description("Excel")]
        Excel = 1,

        /// <summary>
        /// Comma-Separated Values (CSV)
        /// </summary>
        [Description("CSV")]
        Csv = 2,

        /// <summary>
        /// JavaScript Object Notation (JSON)
        /// </summary>
        [Description("JSON")]
        Json = 3,

        /// <summary>
        /// HyperText Markup Language (HTML)
        /// </summary>
        [Description("HTML")]
        Html = 4
    }
}
