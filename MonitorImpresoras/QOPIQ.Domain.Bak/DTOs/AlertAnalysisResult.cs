using System;
using System.Collections.Generic;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Domain.DTOs
{
    /// <summary>
    /// Represents the result of analyzing alerts in a report
    /// </summary>
    public class AlertAnalysisResult
    {
        /// <summary>
        /// Total number of alerts found
        /// </summary>
        public int TotalAlerts { get; set; }

        /// <summary>
        /// Number of critical alerts
        /// </summary>
        public int CriticalCount { get; set; }

        /// <summary>
        /// Number of warning alerts
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// Number of informational alerts
        /// </summary>
        public int InfoCount { get; set; }

        /// <summary>
        /// List of unique alert types found
        /// </summary>
        public List<AlertType> AlertTypes { get; set; } = new();

        /// <summary>
        /// Timestamp of the analysis
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indicates if any critical alerts were found
        /// </summary>
        public bool HasCriticalAlerts => CriticalCount > 0;
    }
}
