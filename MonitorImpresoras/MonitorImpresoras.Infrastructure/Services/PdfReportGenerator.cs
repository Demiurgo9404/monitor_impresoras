using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using System.Text;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación del generador de reportes PDF usando una librería simple
    /// Nota: En producción se recomienda usar QuestPDF, iTextSharp o similar
    /// </summary>
    public class PdfReportGenerator : IPdfReportGenerator
    {
        private readonly ILogger<PdfReportGenerator> _logger;

        public PdfReportGenerator(ILogger<PdfReportGenerator> logger)
        {
            _logger = logger;
        }

        public async Task<byte[]> GeneratePdfReportAsync(ReportDataDto reportData)
        {
            try
            {
                _logger.LogInformation("Generating PDF report for project {ProjectName}", reportData.ProjectName);

                // Por simplicidad, generamos un HTML que simula un PDF
                // En producción, usar QuestPDF o iTextSharp
                var htmlContent = GenerateHtmlReport(reportData);
                
                // Simulamos la conversión a PDF devolviendo el HTML como bytes
                // En producción: usar HtmlToPdf converter o librería PDF nativa
                var pdfBytes = Encoding.UTF8.GetBytes(htmlContent);

                _logger.LogInformation("PDF report generated successfully, size: {Size} bytes", pdfBytes.Length);
                
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                throw;
            }
        }

        public async Task<byte[]> GenerateExecutiveSummaryPdfAsync(ReportDataDto reportData)
        {
            try
            {
                var htmlContent = GenerateExecutiveSummaryHtml(reportData);
                var pdfBytes = Encoding.UTF8.GetBytes(htmlContent);
                
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating executive summary PDF");
                throw;
            }
        }

        public async Task<byte[]> GenerateDetailedPrinterReportAsync(ReportDataDto reportData)
        {
            try
            {
                var htmlContent = GenerateDetailedPrinterHtml(reportData);
                var pdfBytes = Encoding.UTF8.GetBytes(htmlContent);
                
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating detailed printer PDF");
                throw;
            }
        }

        public async Task<byte[]> GenerateConsumableReportAsync(ReportDataDto reportData)
        {
            try
            {
                var htmlContent = GenerateConsumableHtml(reportData);
                var pdfBytes = Encoding.UTF8.GetBytes(htmlContent);
                
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating consumable PDF");
                throw;
            }
        }

        public async Task<byte[]> GenerateCostReportAsync(ReportDataDto reportData)
        {
            try
            {
                var htmlContent = GenerateCostHtml(reportData);
                var pdfBytes = Encoding.UTF8.GetBytes(htmlContent);
                
                return await Task.FromResult(pdfBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cost PDF");
                throw;
            }
        }

        private string GenerateHtmlReport(ReportDataDto reportData)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>QOPIQ - Reporte de Impresoras</title>");
            html.AppendLine("<style>");
            html.AppendLine(GetReportStyles());
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>QOPIQ - Sistema de Monitoreo de Impresoras</h1>");
            html.AppendLine($"<h2>Reporte de {reportData.ProjectName}</h2>");
            html.AppendLine($"<p><strong>Empresa:</strong> {reportData.CompanyName}</p>");
            html.AppendLine($"<p><strong>Período:</strong> {reportData.PeriodStart:dd/MM/yyyy} - {reportData.PeriodEnd:dd/MM/yyyy}</p>");
            html.AppendLine($"<p><strong>Generado:</strong> {reportData.GeneratedAt:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine("</div>");

            // Resumen ejecutivo
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h3>📊 Resumen Ejecutivo</h3>");
            html.AppendLine("<div class='summary-grid'>");
            html.AppendLine($"<div class='summary-item'><span class='label'>Total Impresoras:</span><span class='value'>{reportData.Summary.TotalPrinters}</span></div>");
            html.AppendLine($"<div class='summary-item'><span class='label'>Impresoras Activas:</span><span class='value'>{reportData.Summary.ActivePrinters}</span></div>");
            html.AppendLine($"<div class='summary-item'><span class='label'>Páginas Impresas:</span><span class='value'>{reportData.Summary.TotalPagesPrinted:N0}</span></div>");
            html.AppendLine($"<div class='summary-item'><span class='label'>Escaneos Totales:</span><span class='value'>{reportData.Summary.TotalScans:N0}</span></div>");
            html.AppendLine($"<div class='summary-item'><span class='label'>Tiempo Activo Promedio:</span><span class='value'>{reportData.Summary.AverageUptimePercentage:F1}%</span></div>");
            html.AppendLine($"<div class='summary-item'><span class='label'>Alertas Críticas:</span><span class='value'>{reportData.Summary.CriticalAlerts}</span></div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Detalle por impresora
            if (reportData.Printers.Any())
            {
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h3>🖨️ Detalle por Impresora</h3>");
                html.AppendLine("<table class='printer-table'>");
                html.AppendLine("<thead>");
                html.AppendLine("<tr>");
                html.AppendLine("<th>Impresora</th>");
                html.AppendLine("<th>Modelo</th>");
                html.AppendLine("<th>Ubicación</th>");
                html.AppendLine("<th>Estado</th>");
                html.AppendLine("<th>Páginas B/N</th>");
                html.AppendLine("<th>Páginas Color</th>");
                html.AppendLine("<th>Escaneos</th>");
                html.AppendLine("<th>Tiempo Activo</th>");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");

                foreach (var printer in reportData.Printers)
                {
                    var statusClass = printer.IsOnline ? "status-online" : "status-offline";
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td><strong>{printer.Name}</strong><br><small>{printer.SerialNumber}</small></td>");
                    html.AppendLine($"<td>{printer.Model}</td>");
                    html.AppendLine($"<td>{printer.Location}</td>");
                    html.AppendLine($"<td><span class='{statusClass}'>{printer.Status}</span></td>");
                    html.AppendLine($"<td>{printer.PagesPrintedBW:N0}</td>");
                    html.AppendLine($"<td>{printer.PagesPrintedColor:N0}</td>");
                    html.AppendLine($"<td>{printer.TotalScans:N0}</td>");
                    html.AppendLine($"<td>{printer.UptimePercentage:F1}%</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
                html.AppendLine("</div>");
            }

            // Consumibles
            if (reportData.Consumables.Any())
            {
                html.AppendLine("<div class='section'>");
                html.AppendLine("<h3>🔋 Estado de Consumibles</h3>");
                html.AppendLine("<table class='consumable-table'>");
                html.AppendLine("<thead>");
                html.AppendLine("<tr>");
                html.AppendLine("<th>Impresora</th>");
                html.AppendLine("<th>Tipo</th>");
                html.AppendLine("<th>Color</th>");
                html.AppendLine("<th>Nivel Actual</th>");
                html.AppendLine("<th>Estado</th>");
                html.AppendLine("<th>Días Restantes</th>");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");

                foreach (var consumable in reportData.Consumables)
                {
                    var statusClass = consumable.Status switch
                    {
                        "Critical" => "status-critical",
                        "Low" => "status-warning",
                        _ => "status-ok"
                    };

                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{consumable.PrinterName}</td>");
                    html.AppendLine($"<td>{consumable.ConsumableType}</td>");
                    html.AppendLine($"<td>{consumable.Color}</td>");
                    html.AppendLine($"<td>{consumable.CurrentLevel}%</td>");
                    html.AppendLine($"<td><span class='{statusClass}'>{consumable.Status}</span></td>");
                    html.AppendLine($"<td>{consumable.EstimatedDaysRemaining ?? 0}</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
                html.AppendLine("</div>");
            }

            // Costos
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h3>💰 Análisis de Costos</h3>");
            html.AppendLine("<div class='cost-grid'>");
            html.AppendLine($"<div class='cost-item'><span class='label'>Costo Total B/N:</span><span class='value'>${reportData.Costs.TotalCostBW:F2}</span></div>");
            html.AppendLine($"<div class='cost-item'><span class='label'>Costo Total Color:</span><span class='value'>${reportData.Costs.TotalCostColor:F2}</span></div>");
            html.AppendLine($"<div class='cost-item'><span class='label'>Costo Mantenimiento:</span><span class='value'>${reportData.Costs.TotalMaintenanceCost:F2}</span></div>");
            html.AppendLine($"<div class='cost-item'><span class='label'>Costo Consumibles:</span><span class='value'>${reportData.Costs.TotalConsumableCost:F2}</span></div>");
            html.AppendLine($"<div class='cost-item'><span class='label'>Costo por Página B/N:</span><span class='value'>${reportData.Costs.CostPerPageBW:F4}</span></div>");
            html.AppendLine($"<div class='cost-item'><span class='label'>Costo por Página Color:</span><span class='value'>${reportData.Costs.CostPerPageColor:F4}</span></div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>Generado por QOPIQ - Sistema de Monitoreo de Impresoras</p>");
            html.AppendLine($"<p>Fecha de generación: {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        private string GenerateExecutiveSummaryHtml(ReportDataDto reportData)
        {
            // Versión simplificada para resumen ejecutivo
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><meta charset='utf-8'><title>Resumen Ejecutivo</title>");
            html.AppendLine("<style>" + GetReportStyles() + "</style></head><body>");
            
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>📊 Resumen Ejecutivo</h1>");
            html.AppendLine($"<h2>{reportData.ProjectName}</h2>");
            html.AppendLine("</div>");

            html.AppendLine("<div class='executive-summary'>");
            html.AppendLine($"<p><strong>Total de impresoras monitoreadas:</strong> {reportData.Summary.TotalPrinters}</p>");
            html.AppendLine($"<p><strong>Disponibilidad promedio:</strong> {reportData.Summary.AverageUptimePercentage:F1}%</p>");
            html.AppendLine($"<p><strong>Páginas impresas en el período:</strong> {reportData.Summary.TotalPagesPrinted:N0}</p>");
            html.AppendLine($"<p><strong>Costo total estimado:</strong> ${reportData.Costs.TotalCostBW + reportData.Costs.TotalCostColor:F2}</p>");
            html.AppendLine("</div>");

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        private string GenerateDetailedPrinterHtml(ReportDataDto reportData)
        {
            // Implementación similar pero enfocada en detalles de impresoras
            return GenerateHtmlReport(reportData); // Por simplicidad, usar el reporte completo
        }

        private string GenerateConsumableHtml(ReportDataDto reportData)
        {
            // Implementación enfocada en consumibles
            return GenerateHtmlReport(reportData); // Por simplicidad, usar el reporte completo
        }

        private string GenerateCostHtml(ReportDataDto reportData)
        {
            // Implementación enfocada en costos
            return GenerateHtmlReport(reportData); // Por simplicidad, usar el reporte completo
        }

        private string GetReportStyles()
        {
            return @"
                body { font-family: Arial, sans-serif; margin: 20px; color: #333; }
                .header { text-align: center; border-bottom: 2px solid #2563eb; padding-bottom: 20px; margin-bottom: 30px; }
                .header h1 { color: #2563eb; margin: 0; }
                .header h2 { color: #64748b; margin: 10px 0; }
                .section { margin-bottom: 30px; }
                .section h3 { color: #1e40af; border-left: 4px solid #3b82f6; padding-left: 10px; }
                .summary-grid, .cost-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 15px; margin: 20px 0; }
                .summary-item, .cost-item { background: #f8fafc; padding: 15px; border-radius: 8px; border-left: 4px solid #3b82f6; }
                .summary-item .label, .cost-item .label { display: block; font-weight: bold; color: #64748b; margin-bottom: 5px; }
                .summary-item .value, .cost-item .value { display: block; font-size: 1.5em; font-weight: bold; color: #1e40af; }
                table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e2e8f0; }
                th { background-color: #f1f5f9; font-weight: bold; color: #1e40af; }
                tr:hover { background-color: #f8fafc; }
                .status-online { color: #059669; font-weight: bold; }
                .status-offline { color: #dc2626; font-weight: bold; }
                .status-ok { color: #059669; font-weight: bold; }
                .status-warning { color: #d97706; font-weight: bold; }
                .status-critical { color: #dc2626; font-weight: bold; }
                .footer { text-align: center; margin-top: 40px; padding-top: 20px; border-top: 1px solid #e2e8f0; color: #64748b; font-size: 0.9em; }
                .executive-summary { background: #f0f9ff; padding: 20px; border-radius: 8px; border-left: 4px solid #0ea5e9; }
                .executive-summary p { margin: 10px 0; font-size: 1.1em; }
            ";
        }
    }
}
