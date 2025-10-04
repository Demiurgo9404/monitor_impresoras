using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Text;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del generador de reportes Excel
    /// Nota: En producción se recomienda usar EPPlus, ClosedXML o similar
    /// </summary>
    public class ExcelReportGenerator : IExcelReportGenerator
    {
        private readonly ILogger<ExcelReportGenerator> _logger;

        public ExcelReportGenerator(ILogger<ExcelReportGenerator> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GenerateExcelReportAsync(ReportDataDto reportData)
        {
            try
            {
                _logger.LogInformation("Generating Excel report for project {ProjectName}", reportData.ProjectName);

                // Por simplicidad, generamos un CSV que simula Excel
                // En producción: usar EPPlus o ClosedXML para generar .xlsx real
                var csvContent = GenerateMultiSheetCsv(reportData);
                var excelBytes = Encoding.UTF8.GetBytes(csvContent);

                _logger.LogInformation("Excel report generated successfully, size: {Size} bytes", excelBytes.Length);
                
                return await Task.FromResult(excelBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw;
            }
        }

        public async Task<byte[]> GenerateSummaryExcelAsync(ReportDataDto reportData)
        {
            try
            {
                var csvContent = GenerateSummaryCsv(reportData);
                var excelBytes = Encoding.UTF8.GetBytes(csvContent);
                
                return await Task.FromResult(excelBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary Excel");
                throw;
            }
        }

        public async Task<byte[]> GeneratePrinterDetailsExcelAsync(ReportDataDto reportData)
        {
            try
            {
                var csvContent = GeneratePrinterDetailsCsv(reportData);
                var excelBytes = Encoding.UTF8.GetBytes(csvContent);
                
                return await Task.FromResult(excelBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating printer details Excel");
                throw;
            }
        }

        public async Task<byte[]> GenerateConsumablesExcelAsync(ReportDataDto reportData)
        {
            try
            {
                var csvContent = GenerateConsumablesCsv(reportData);
                var excelBytes = Encoding.UTF8.GetBytes(csvContent);
                
                return await Task.FromResult(excelBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating consumables Excel");
                throw;
            }
        }

        public async Task<byte[]> GenerateCostAnalysisExcelAsync(ReportDataDto reportData)
        {
            try
            {
                var csvContent = GenerateCostAnalysisCsv(reportData);
                var excelBytes = Encoding.UTF8.GetBytes(csvContent);
                
                return await Task.FromResult(excelBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cost analysis Excel");
                throw;
            }
        }

        public async Task<byte[]> GenerateCsvReportAsync(ReportDataDto reportData)
        {
            try
            {
                var csvContent = GenerateSimpleCsv(reportData);
                var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                
                return await Task.FromResult(csvBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSV report");
                throw;
            }
        }

        private string GenerateMultiSheetCsv(ReportDataDto reportData)
        {
            var csv = new StringBuilder();
            
            // Hoja 1: Resumen
            csv.AppendLine("=== HOJA: RESUMEN EJECUTIVO ===");
            csv.AppendLine($"Proyecto,{EscapeCsv(reportData.ProjectName)}");
            csv.AppendLine($"Empresa,{EscapeCsv(reportData.CompanyName)}");
            csv.AppendLine($"Período Inicio,{reportData.PeriodStart:yyyy-MM-dd}");
            csv.AppendLine($"Período Fin,{reportData.PeriodEnd:yyyy-MM-dd}");
            csv.AppendLine($"Generado,{reportData.GeneratedAt:yyyy-MM-dd HH:mm}");
            csv.AppendLine();
            
            csv.AppendLine("Métrica,Valor");
            csv.AppendLine($"Total Impresoras,{reportData.Summary.TotalPrinters}");
            csv.AppendLine($"Impresoras Activas,{reportData.Summary.ActivePrinters}");
            csv.AppendLine($"Impresoras Inactivas,{reportData.Summary.InactivePrinters}");
            csv.AppendLine($"Total Páginas Impresas,{reportData.Summary.TotalPagesPrinted}");
            csv.AppendLine($"Páginas Blanco y Negro,{reportData.Summary.TotalPagesBlackWhite}");
            csv.AppendLine($"Páginas Color,{reportData.Summary.TotalPagesColor}");
            csv.AppendLine($"Total Escaneos,{reportData.Summary.TotalScans}");
            csv.AppendLine($"Total Copias,{reportData.Summary.TotalCopies}");
            csv.AppendLine($"Tiempo Activo Promedio (%),{reportData.Summary.AverageUptimePercentage:F2}");
            csv.AppendLine($"Total Alertas,{reportData.Summary.TotalAlerts}");
            csv.AppendLine($"Alertas Críticas,{reportData.Summary.CriticalAlerts}");
            csv.AppendLine();

            // Hoja 2: Detalle de Impresoras
            csv.AppendLine("=== HOJA: DETALLE DE IMPRESORAS ===");
            csv.AppendLine("Nombre,Modelo,Número Serie,Ubicación,Estado,En Línea,Páginas B/N,Páginas Color,Escaneos,Copias,Tóner Negro (%),Tóner Cian (%),Tóner Magenta (%),Tóner Amarillo (%),Fusor (%),Tiempo Activo (%),Errores,Último Mantenimiento,Necesita Mantenimiento");
            
            foreach (var printer in reportData.Printers)
            {
                csv.AppendLine($"{EscapeCsv(printer.Name)},{EscapeCsv(printer.Model)},{EscapeCsv(printer.SerialNumber)},{EscapeCsv(printer.Location)},{EscapeCsv(printer.Status)},{(printer.IsOnline ? "Sí" : "No")},{printer.PagesPrintedBW},{printer.PagesPrintedColor},{printer.TotalScans},{printer.TotalCopies},{printer.TonerBlackLevel ?? 0},{printer.TonerCyanLevel ?? 0},{printer.TonerMagentaLevel ?? 0},{printer.TonerYellowLevel ?? 0},{printer.FuserLevel ?? 0},{printer.UptimePercentage:F2},{printer.ErrorCount},{printer.LastMaintenance?.ToString("yyyy-MM-dd") ?? "N/A"},{(printer.NeedsMaintenance ? "Sí" : "No")}");
            }
            csv.AppendLine();

            // Hoja 3: Consumibles
            csv.AppendLine("=== HOJA: ESTADO DE CONSUMIBLES ===");
            csv.AppendLine("Impresora,Tipo Consumible,Color,Nivel Actual (%),Estado,Último Reemplazo,Días Restantes Estimados,Costo Reemplazo");
            
            foreach (var consumable in reportData.Consumables)
            {
                csv.AppendLine($"{EscapeCsv(consumable.PrinterName)},{EscapeCsv(consumable.ConsumableType)},{EscapeCsv(consumable.Color)},{consumable.CurrentLevel ?? 0},{EscapeCsv(consumable.Status)},{consumable.LastReplaced?.ToString("yyyy-MM-dd") ?? "N/A"},{consumable.EstimatedDaysRemaining ?? 0},{consumable.ReplacementCost ?? 0:F2}");
            }
            csv.AppendLine();

            // Hoja 4: Análisis de Costos
            csv.AppendLine("=== HOJA: ANÁLISIS DE COSTOS ===");
            csv.AppendLine("Concepto,Valor");
            csv.AppendLine($"Costo Total B/N,${reportData.Costs.TotalCostBW:F2}");
            csv.AppendLine($"Costo Total Color,${reportData.Costs.TotalCostColor:F2}");
            csv.AppendLine($"Costo Mantenimiento,${reportData.Costs.TotalMaintenanceCost:F2}");
            csv.AppendLine($"Costo Consumibles,${reportData.Costs.TotalConsumableCost:F2}");
            csv.AppendLine($"Costo por Página B/N,${reportData.Costs.CostPerPageBW:F4}");
            csv.AppendLine($"Costo por Página Color,${reportData.Costs.CostPerPageColor:F4}");
            csv.AppendLine($"Costo Mensual Promedio,${reportData.Costs.AverageMonthlyCost:F2}");
            csv.AppendLine();

            // Costos mensuales
            if (reportData.Costs.MonthlyCosts.Any())
            {
                csv.AppendLine("Año,Mes,Costo B/N,Costo Color,Costo Mantenimiento,Costo Consumibles,Costo Total,Páginas B/N,Páginas Color");
                foreach (var monthlyCost in reportData.Costs.MonthlyCosts)
                {
                    csv.AppendLine($"{monthlyCost.Year},{monthlyCost.Month},{monthlyCost.CostBW:F2},{monthlyCost.CostColor:F2},{monthlyCost.MaintenanceCost:F2},{monthlyCost.ConsumableCost:F2},{monthlyCost.TotalCost:F2},{monthlyCost.PagesBW},{monthlyCost.PagesColor}");
                }
            }

            return csv.ToString();
        }

        private string GenerateSummaryCsv(ReportDataDto reportData)
        {
            var csv = new StringBuilder();
            
            csv.AppendLine("QOPIQ - Resumen Ejecutivo");
            csv.AppendLine($"Proyecto: {reportData.ProjectName}");
            csv.AppendLine($"Empresa: {reportData.CompanyName}");
            csv.AppendLine($"Período: {reportData.PeriodStart:dd/MM/yyyy} - {reportData.PeriodEnd:dd/MM/yyyy}");
            csv.AppendLine();
            
            csv.AppendLine("Métrica,Valor");
            csv.AppendLine($"Total Impresoras,{reportData.Summary.TotalPrinters}");
            csv.AppendLine($"Disponibilidad Promedio,{reportData.Summary.AverageUptimePercentage:F1}%");
            csv.AppendLine($"Páginas Impresas,{reportData.Summary.TotalPagesPrinted:N0}");
            csv.AppendLine($"Costo Total Estimado,${reportData.Costs.TotalCostBW + reportData.Costs.TotalCostColor:F2}");

            return csv.ToString();
        }

        private string GeneratePrinterDetailsCsv(ReportDataDto reportData)
        {
            var csv = new StringBuilder();
            
            csv.AppendLine("QOPIQ - Detalle de Impresoras");
            csv.AppendLine($"Proyecto: {reportData.ProjectName}");
            csv.AppendLine($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}");
            csv.AppendLine();
            
            csv.AppendLine("Nombre,Modelo,Serie,Ubicación,Estado,Páginas B/N,Páginas Color,Escaneos,Tiempo Activo %");
            
            foreach (var printer in reportData.Printers)
            {
                csv.AppendLine($"{EscapeCsv(printer.Name)},{EscapeCsv(printer.Model)},{EscapeCsv(printer.SerialNumber)},{EscapeCsv(printer.Location)},{EscapeCsv(printer.Status)},{printer.PagesPrintedBW},{printer.PagesPrintedColor},{printer.TotalScans},{printer.UptimePercentage:F1}");
            }

            return csv.ToString();
        }

        private string GenerateConsumablesCsv(ReportDataDto reportData)
        {
            var csv = new StringBuilder();
            
            csv.AppendLine("QOPIQ - Estado de Consumibles");
            csv.AppendLine($"Proyecto: {reportData.ProjectName}");
            csv.AppendLine($"Generado: {reportData.GeneratedAt:dd/MM/yyyy HH:mm}");
            csv.AppendLine();
            
            csv.AppendLine("Impresora,Tipo,Color,Nivel %,Estado,Días Restantes");
            
            foreach (var consumable in reportData.Consumables)
            {
                csv.AppendLine($"{EscapeCsv(consumable.PrinterName)},{EscapeCsv(consumable.ConsumableType)},{EscapeCsv(consumable.Color)},{consumable.CurrentLevel ?? 0},{EscapeCsv(consumable.Status)},{consumable.EstimatedDaysRemaining ?? 0}");
            }

            return csv.ToString();
        }

        private string GenerateCostAnalysisCsv(ReportDataDto reportData)
        {
            var csv = new StringBuilder();
            
            csv.AppendLine("QOPIQ - Análisis de Costos");
            csv.AppendLine($"Proyecto: {reportData.ProjectName}");
            csv.AppendLine($"Período: {reportData.PeriodStart:dd/MM/yyyy} - {reportData.PeriodEnd:dd/MM/yyyy}");
            csv.AppendLine();
            
            csv.AppendLine("Concepto,Valor");
            csv.AppendLine($"Costo Total B/N,${reportData.Costs.TotalCostBW:F2}");
            csv.AppendLine($"Costo Total Color,${reportData.Costs.TotalCostColor:F2}");
            csv.AppendLine($"Costo Mantenimiento,${reportData.Costs.TotalMaintenanceCost:F2}");
            csv.AppendLine($"Costo por Página B/N,${reportData.Costs.CostPerPageBW:F4}");
            csv.AppendLine($"Costo por Página Color,${reportData.Costs.CostPerPageColor:F4}");

            return csv.ToString();
        }

        private string GenerateSimpleCsv(ReportDataDto reportData)
        {
            var csv = new StringBuilder();
            
            csv.AppendLine("Impresora,Modelo,Ubicación,Estado,Páginas B/N,Páginas Color,Escaneos,Tiempo Activo %");
            
            foreach (var printer in reportData.Printers)
            {
                csv.AppendLine($"{EscapeCsv(printer.Name)},{EscapeCsv(printer.Model)},{EscapeCsv(printer.Location)},{EscapeCsv(printer.Status)},{printer.PagesPrintedBW},{printer.PagesPrintedColor},{printer.TotalScans},{printer.UptimePercentage:F1}");
            }

            return csv.ToString();
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            // Escapar comillas y comas en CSV
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}
