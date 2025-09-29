using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace MonitorImpresoras.Application.Services.Export
{
    /// <summary>
    /// Servicio para exportación de reportes a formato Excel
    /// </summary>
    public class ExcelExportService
    {
        private readonly ILogger<ExcelExportService> _logger;

        public ExcelExportService(ILogger<ExcelExportService> logger)
        {
            _logger = logger;

            // Configurar EPPlus para no requerir licencia
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Genera un Excel a partir de datos de reporte
        /// </summary>
        public async Task<byte[]> GenerateExcelAsync(
            string reportTitle,
            string reportDescription,
            IEnumerable<object> data,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            _logger.LogInformation("Generando Excel: {ReportTitle} para usuario: {UserName}", reportTitle, userName);

            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Reporte");

                // Información del reporte
                worksheet.Cells[1, 1].Value = reportTitle;
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.Blue);

                worksheet.Cells[2, 1].Value = reportDescription;
                worksheet.Cells[2, 1, 2, 6].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Size = 12;
                worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);

                // Información de generación
                worksheet.Cells[3, 1].Value = $"Generado por: {userName}";
                worksheet.Cells[3, 4].Value = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells[3, 1, 3, 2].Style.Font.Size = 10;
                worksheet.Cells[3, 4, 3, 5].Style.Font.Size = 10;

                int currentRow = 4;

                // Agregar parámetros si existen
                if (parameters?.Any() == true)
                {
                    worksheet.Cells[currentRow, 1].Value = "Parámetros del Reporte";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                    currentRow++;

                    foreach (var param in parameters)
                    {
                        worksheet.Cells[currentRow, 1].Value = param.Key;
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 2].Value = param.Value?.ToString() ?? "N/A";
                        currentRow++;
                    }

                    currentRow++; // Espacio en blanco
                }

                // Agregar datos si existen
                if (data.Any())
                {
                    var dataList = data.ToList();
                    var firstItem = dataList.First();
                    var properties = firstItem.GetType().GetProperties();

                    // Headers
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var cell = worksheet.Cells[currentRow, col + 1];
                        cell.Value = properties[col].Name;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    currentRow++;

                    // Data rows
                    foreach (var item in dataList)
                    {
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var cell = worksheet.Cells[currentRow, col + 1];
                            var value = GetPropertyValue(item, properties[col]);

                            cell.Value = value;

                            // Formato condicional basado en el tipo de dato
                            ApplyConditionalFormatting(cell, properties[col].Name, value);
                        }
                        currentRow++;
                    }

                    // Autosize columns
                    for (int col = 1; col <= properties.Length; col++)
                    {
                        worksheet.Column(col).AutoFit();
                        if (worksheet.Column(col).Width > 50) // Máximo ancho
                        {
                            worksheet.Column(col).Width = 50;
                        }
                    }

                    // Agregar filtros
                    var endColumn = properties.Length;
                    worksheet.Cells[5, 1, currentRow - 1, endColumn].AutoFilter = true;
                }
                else
                {
                    worksheet.Cells[currentRow, 1].Value = "No hay datos disponibles para este reporte";
                    worksheet.Cells[currentRow, 1].Style.Font.Color.SetColor(Color.Gray);
                }

                var excelBytes = await package.GetAsByteArrayAsync();
                _logger.LogInformation("Excel generado exitosamente: {ReportTitle}, Tamaño: {Size} bytes", reportTitle, excelBytes.Length);

                return excelBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar Excel: {ReportTitle}", reportTitle);
                throw;
            }
        }

        /// <summary>
        /// Genera un Excel de impresoras con formato específico
        /// </summary>
        public async Task<byte[]> GeneratePrinterExcelAsync(
            IEnumerable<object> printerData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Impresoras";
            var reportDescription = "Estado y configuración de las impresoras del sistema";

            return await GenerateExcelAsync(reportTitle, reportDescription, printerData, userName, parameters);
        }

        /// <summary>
        /// Genera un Excel de usuarios con formato específico
        /// </summary>
        public async Task<byte[]> GenerateUserExcelAsync(
            IEnumerable<object> userData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Usuarios";
            var reportDescription = "Lista de usuarios del sistema con roles y permisos";

            return await GenerateExcelAsync(reportTitle, reportDescription, userData, userName, parameters);
        }

        /// <summary>
        /// Genera un Excel de auditoría con formato específico
        /// </summary>
        public async Task<byte[]> GenerateAuditExcelAsync(
            IEnumerable<object> auditData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Auditoría";
            var reportDescription = "Logs de auditoría y eventos de seguridad del sistema";

            return await GenerateExcelAsync(reportTitle, reportDescription, auditData, userName, parameters);
        }

        /// <summary>
        /// Genera un Excel de permisos con formato específico
        /// </summary>
        public async Task<byte[]> GeneratePermissionsExcelAsync(
            IEnumerable<object> permissionsData,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            var reportTitle = "Reporte de Permisos";
            var reportDescription = "Claims y permisos granulares asignados a usuarios";

            return await GenerateExcelAsync(reportTitle, reportDescription, permissionsData, userName, parameters);
        }

        /// <summary>
        /// Genera un Excel con múltiples hojas
        /// </summary>
        public async Task<byte[]> GenerateMultiSheetExcelAsync(
            Dictionary<string, (IEnumerable<object> Data, string Description)> sheets,
            string reportTitle,
            string userName,
            Dictionary<string, object>? parameters = null)
        {
            _logger.LogInformation("Generando Excel multi-hoja: {ReportTitle} para usuario: {UserName}", reportTitle, userName);

            try
            {
                using var package = new ExcelPackage();

                foreach (var sheet in sheets)
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheet.Key);

                    // Título de la hoja
                    worksheet.Cells[1, 1].Value = $"{reportTitle} - {sheet.Key}";
                    worksheet.Cells[1, 1, 1, 6].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Color.SetColor(Color.Blue);

                    worksheet.Cells[2, 1].Value = sheet.Value.Description;
                    worksheet.Cells[2, 1, 2, 6].Merge = true;
                    worksheet.Cells[2, 1].Style.Font.Size = 12;
                    worksheet.Cells[2, 1].Style.Font.Color.SetColor(Color.Gray);

                    // Información de generación
                    worksheet.Cells[3, 1].Value = $"Generado por: {userName}";
                    worksheet.Cells[3, 4].Value = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Cells[3, 1, 3, 2].Style.Font.Size = 10;
                    worksheet.Cells[3, 4, 3, 5].Style.Font.Size = 10;

                    int currentRow = 4;

                    // Agregar parámetros si existen
                    if (parameters?.Any() == true)
                    {
                        worksheet.Cells[currentRow, 1].Value = "Parámetros del Reporte";
                        worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                        currentRow++;

                        foreach (var param in parameters)
                        {
                            worksheet.Cells[currentRow, 1].Value = param.Key;
                            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                            worksheet.Cells[currentRow, 2].Value = param.Value?.ToString() ?? "N/A";
                            currentRow++;
                        }

                        currentRow++;
                    }

                    // Agregar datos
                    if (sheet.Value.Data.Any())
                    {
                        var dataList = sheet.Value.Data.ToList();
                        var firstItem = dataList.First();
                        var properties = firstItem.GetType().GetProperties();

                        // Headers
                        for (int col = 0; col < properties.Length; col++)
                        {
                            var cell = worksheet.Cells[currentRow, col + 1];
                            cell.Value = properties[col].Name;
                            cell.Style.Font.Bold = true;
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        currentRow++;

                        // Data rows
                        foreach (var item in dataList)
                        {
                            for (int col = 0; col < properties.Length; col++)
                            {
                                var cell = worksheet.Cells[currentRow, col + 1];
                                var value = GetPropertyValue(item, properties[col]);

                                cell.Value = value;
                                ApplyConditionalFormatting(cell, properties[col].Name, value);
                            }
                            currentRow++;
                        }

                        // Autosize columns
                        for (int col = 1; col <= properties.Length; col++)
                        {
                            worksheet.Column(col).AutoFit();
                            if (worksheet.Column(col).Width > 50)
                            {
                                worksheet.Column(col).Width = 50;
                            }
                        }
                    }
                }

                var excelBytes = await package.GetAsByteArrayAsync();
                _logger.LogInformation("Excel multi-hoja generado exitosamente: {ReportTitle}, Tamaño: {Size} bytes",
                    reportTitle, excelBytes.Length);

                return excelBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar Excel multi-hoja: {ReportTitle}", reportTitle);
                throw;
            }
        }

        /// <summary>
        /// Aplica formato condicional a celdas según el tipo de dato
        /// </summary>
        private void ApplyConditionalFormatting(ExcelRange cell, string propertyName, string value)
        {
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            // Formato condicional para impresoras
            if (propertyName.Contains("Status", StringComparison.OrdinalIgnoreCase))
            {
                switch (value.ToLower())
                {
                    case "active":
                        cell.Style.Font.Color.SetColor(Color.Green);
                        break;
                    case "maintenance":
                    case "offline":
                        cell.Style.Font.Color.SetColor(Color.Orange);
                        break;
                    case "error":
                        cell.Style.Font.Color.SetColor(Color.Red);
                        break;
                }
            }

            // Formato condicional para niveles de tóner/papel
            if (propertyName.Contains("Level", StringComparison.OrdinalIgnoreCase) ||
                propertyName.Contains("Toner", StringComparison.OrdinalIgnoreCase) ||
                propertyName.Contains("Paper", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(value.Replace("%", ""), out double level))
                {
                    if (level < 10)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.Red);
                        cell.Style.Font.Color.SetColor(Color.White);
                    }
                    else if (level < 25)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                        cell.Style.Font.Color.SetColor(Color.White);
                    }
                    else if (level > 80)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.Green);
                        cell.Style.Font.Color.SetColor(Color.White);
                    }
                }
            }

            // Formato condicional para fechas
            if (propertyName.Contains("Date", StringComparison.OrdinalIgnoreCase) ||
                propertyName.Contains("At", StringComparison.OrdinalIgnoreCase))
            {
                if (DateTime.TryParse(value, out _))
                {
                    cell.Style.Numberformat.Format = "dd/mm/yyyy hh:mm";
                }
            }

            // Formato condicional para booleanos
            if (propertyName.Contains("Active", StringComparison.OrdinalIgnoreCase) ||
                propertyName.Contains("Enabled", StringComparison.OrdinalIgnoreCase))
            {
                switch (value.ToLower())
                {
                    case "true":
                    case "activo":
                    case "habilitado":
                        cell.Style.Font.Color.SetColor(Color.Green);
                        break;
                    case "false":
                    case "inactivo":
                    case "deshabilitado":
                        cell.Style.Font.Color.SetColor(Color.Red);
                        break;
                }
            }
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
        /// Genera nombre de archivo para Excel
        /// </summary>
        public string GenerateFileName(string reportTitle, string format = "xlsx")
        {
            var cleanTitle = string.Concat(reportTitle.Where(c => !char.IsPunctuation(c) || c == ' ' || c == '-'))
                .Replace(" ", "_")
                .ToLower();

            return $"{cleanTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.{format}";
        }
    }
}
