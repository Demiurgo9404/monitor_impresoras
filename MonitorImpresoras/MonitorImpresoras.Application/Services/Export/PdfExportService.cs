using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Text.Json;

namespace MonitorImpresoras.Application.Services.Export
{
    /// <summary>
    /// Servicio para exportación de reportes a formato PDF
    /// </summary>
    public class PdfExportService
    {
        private readonly ILogger<PdfExportService> _logger;

        public PdfExportService(ILogger<PdfExportService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Genera un PDF a partir de datos de reporte
        /// </summary>
        public async Task<byte[]> GeneratePdfAsync(
            string reportTitle,
            string reportDescription,
            IEnumerable<object> data,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            _logger.LogInformation("Generando PDF: {ReportTitle} para usuario: {UserName}", reportTitle, userName);

            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                        page.Header()
                            .Column(header =>
                            {
                                header.Item().Text(reportTitle).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                                header.Item().Text(reportDescription).FontSize(12).FontColor(Colors.Grey.Darken1);
                                header.Item().PaddingTop(1, Unit.Centimetre);
                                header.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"Generado por: {userName}").FontSize(10);
                                    row.RelativeItem(2).Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                                });
                            });

                        page.Content()
                            .Column(content =>
                            {
                                // Agregar parámetros si existen
                                if (parameters?.Any() == true)
                                {
                                    content.Item().PaddingBottom(1, Unit.Centimetre);
                                    content.Item().Text("Parámetros del Reporte").FontSize(14).Bold();
                                    content.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                    foreach (var param in parameters)
                                    {
                                        content.Item().Row(row =>
                                        {
                                            row.RelativeItem().Text($"{param.Key}:").Bold();
                                            row.RelativeItem(2).Text(param.Value?.ToString() ?? "N/A");
                                        });
                                    }

                                    content.Item().PaddingBottom(1, Unit.Centimetre);
                                }

                                // Agregar tabla de datos
                                if (data.Any())
                                {
                                    content.Item().Text("Datos del Reporte").FontSize(14).Bold();
                                    content.Item().PaddingBottom(0.5f, Unit.Centimetre);

                                    var dataList = data.ToList();
                                    var firstItem = dataList.First();
                                    var properties = firstItem.GetType().GetProperties();

                                    content.Item().Table(table =>
                                    {
                                        // Definir columnas
                                        table.ColumnsDefinition(columns =>
                                        {
                                            foreach (var prop in properties)
                                            {
                                                columns.RelativeColumn();
                                            }
                                        });

                                        // Header de la tabla
                                        table.Header(header =>
                                        {
                                            foreach (var prop in properties)
                                            {
                                                header.Cell().Element(CellStyle).Text(prop.Name).Bold();
                                            }
                                        });

                                        // Filas de datos
                                        foreach (var item in dataList)
                                        {
                                            table.Cell().Element(CellStyle).Text(GetPropertyValue(item, properties[0]));
                                            table.Cell().Element(CellStyle).Text(GetPropertyValue(item, properties[1]));
                                            table.Cell().Element(CellStyle).Text(GetPropertyValue(item, properties[2]));
                                            table.Cell().Element(CellStyle).Text(GetPropertyValue(item, properties[3]));

                                            // Para propiedades adicionales, agregar celdas adicionales
                                            for (int i = 4; i < properties.Length; i++)
                                            {
                                                table.Cell().Element(CellStyle).Text(GetPropertyValue(item, properties[i]));
                                            }
                                        }
                                    });
                                }
                                else
                                {
                                    content.Item().Text("No hay datos disponibles para este reporte").FontSize(12).FontColor(Colors.Grey.Darken1);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("PDF generado exitosamente: {ReportTitle}, Tamaño: {Size} bytes", reportTitle, pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF: {ReportTitle}", reportTitle);
                throw;
            }
        }

        /// <summary>
        /// Genera un PDF de impresoras con formato específico
        /// </summary>
        public async Task<byte[]> GeneratePrinterPdfAsync(
            IEnumerable<object> printerData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Impresoras";
            var reportDescription = "Estado y configuración de las impresoras del sistema";

            return await GeneratePdfAsync(reportTitle, reportDescription, printerData, userName, parameters);
        }

        /// <summary>
        /// Genera un PDF de usuarios con formato específico
        /// </summary>
        public async Task<byte[]> GenerateUserPdfAsync(
            IEnumerable<object> userData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Usuarios";
            var reportDescription = "Lista de usuarios del sistema con roles y permisos";

            return await GeneratePdfAsync(reportTitle, reportDescription, userData, userName, parameters);
        }

        /// <summary>
        /// Genera un PDF de auditoría con formato específico
        /// </summary>
        public async Task<byte[]> GenerateAuditPdfAsync(
            IEnumerable<object> auditData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Auditoría";
            var reportDescription = "Logs de auditoría y eventos de seguridad del sistema";

            return await GeneratePdfAsync(reportTitle, reportDescription, auditData, userName, parameters);
        }

        /// <summary>
        /// Genera un PDF de permisos con formato específico
        /// </summary>
        public async Task<byte[]> GeneratePermissionsPdfAsync(
            IEnumerable<object> permissionsData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Permisos";
            var reportDescription = "Claims y permisos granulares asignados a usuarios";

            return await GeneratePdfAsync(reportTitle, reportDescription, permissionsData, userName, parameters);
        }

        /// <summary>
        /// Estilo para celdas de tabla
        /// </summary>
        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(5)
                .AlignCenter();
        }

        /// <summary>
        /// Obtiene el valor de una propiedad de forma segura
        /// </summary>
        private static string GetPropertyValue(object obj, System.Reflection.PropertyInfo property)
        {
            try
            {
                var value = property.GetValue(obj);
                return value?.ToString() ?? "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        /// <summary>
        /// Genera nombre de archivo para PDF
        /// </summary>
        public string GenerateFileName(string reportTitle, string format = "pdf")
        {
            var cleanTitle = string.Concat(reportTitle.Where(c => !char.IsPunctuation(c) || c == ' ' || c == '-'))
                .Replace(" ", "_")
                .ToLower();

            return $"{cleanTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
        }
    }
}
