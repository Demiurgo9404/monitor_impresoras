using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    public class ReportsController : BaseApiController
    {
        private readonly IReportService _reportService;
        private readonly IPrintJobService _printJobService;
        private readonly IConsumableService _consumableService;
        private readonly ILogger<ReportsController> _logger;
        private readonly IMapper _mapper;

        public ReportsController(
            IReportService reportService,
            IPrintJobService printJobService,
            IConsumableService consumableService,
            ILogger<ReportsController> logger,
            IMapper mapper)
        {
            _reportService = reportService;
            _printJobService = printJobService;
            _consumableService = consumableService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReports(
            [FromQuery] string type = null,
            [FromQuery] string status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new ReportFilterDTO
            {
                Type = type,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var reports = await _reportService.GetReportsByFilterAsync(filter);
            return HandleResult(reports);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<ReportDTO>> GetReport(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            return HandleResult(report);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ReportDTO>> CreateReport([FromBody] CreateReportDTO reportDto)
        {
            var result = await _reportService.CreateReportAsync(reportDto, UserId);
            return CreatedAtAction(nameof(GetReport), new { id = result.Id }, result);
        }

        [HttpPost("generate-usage")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ReportDTO>> GenerateUsageReport([FromBody] ReportFilterDTO filter)
        {
            var report = await _reportService.GeneratePrinterUsageReportAsync(filter);
            return HandleResult(report);
        }

        [HttpPost("generate-cost")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ReportDTO>> GenerateCostReport([FromBody] ReportFilterDTO filter)
        {
            var report = await _reportService.GenerateCostReportAsync(filter);
            return HandleResult(report);
        }

        [HttpPost("generate-consumable")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<ReportDTO>> GenerateConsumableReport([FromBody] ReportFilterDTO filter)
        {
            var report = await _reportService.GenerateConsumableReportAsync(filter);
            return HandleResult(report);
        }

        [HttpGet("{id}/export/pdf")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ExportReportToPdf(int id)
        {
            try
            {
                var pdfBytes = await _reportService.ExportReportToPdfAsync(id);
                return File(pdfBytes, "application/pdf", $"report-{id}.pdf");
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "PDF export not yet implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report {Id} to PDF", id);
                return StatusCode(500, "Error exporting report");
            }
        }

        [HttpGet("{id}/export/excel")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ExportReportToExcel(int id)
        {
            try
            {
                var excelBytes = await _reportService.ExportReportToExcelAsync(id);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report-{id}.xlsx");
            }
            catch (NotImplementedException)
            {
                return StatusCode(501, "Excel export not yet implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report {Id} to Excel", id);
                return StatusCode(500, "Error exporting report");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var result = await _reportService.DeleteReportAsync(id, UserId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("print-jobs/export")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ExportPrintJobs(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? printerId = null,
            [FromQuery] string format = "csv")
        {
            try
            {
                var filter = new PrintJobFilterDTO
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    PrinterId = printerId
                };

                var jobs = await _printJobService.GetPrintJobsByFilterAsync(filter);

                if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csv = GeneratePrintJobsCsv(jobs);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"printjobs-export-{DateTime.UtcNow:yyyyMMdd}.csv");
                }
                else if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
                {
                    // TODO: Implement PDF generation using a library like iTextSharp or QuestPDF
                    return StatusCode(501, "PDF export not yet implemented");
                }

                return BadRequest("Unsupported export format. Use 'csv' or 'pdf'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting print jobs");
                return StatusCode(500, "Error exporting print jobs");
            }
        }

        [HttpGet("consumables/export")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<IActionResult> ExportConsumables(
            [FromQuery] int? printerId = null,
            [FromQuery] string status = null,
            [FromQuery] string format = "csv")
        {
            try
            {
                var filter = new ConsumableFilterDTO
                {
                    PrinterId = printerId,
                    Status = status
                };

                var consumables = await _consumableService.GetConsumablesByFilterAsync(filter);

                if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
                {
                    var csv = GenerateConsumablesCsv(consumables);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"consumables-export-{DateTime.UtcNow:yyyyMMdd}.csv");
                }

                return BadRequest("Unsupported export format. Only 'csv' is supported.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting consumables");
                return StatusCode(500, "Error exporting consumables");
            }
        }

        private string GeneratePrintJobsCsv(IEnumerable<PrintJobDTO> jobs)
        {
            using var writer = new StringWriter();
            writer.WriteLine("ID,Printer,User,Document,Pages,IsColor,IsDuplex,Status,PrintedAt,Cost");

            foreach (var job in jobs)
            {
                writer.WriteLine($"\"{job.Id}\"," +
                    $"\"{EscapeCsv(job.PrinterName)}\"," +
                    $"\"{EscapeCsv(job.UserName)}\"," +
                    $"\"{EscapeCsv(job.DocumentName)}\"," +
                    $"{job.Pages}," +
                    $"{(job.IsColor ? "Yes" : "No")}," +
                    $"{(job.IsDuplex ? "Yes" : "No")}," +
                    $"\"{EscapeCsv(job.JobStatus)}\"," +
                    $"\"{job.PrintedAt:yyyy-MM-dd HH:mm:ss}\"," +
                    $"\"{job.Cost?.ToString("C")}\"");
            }

            return writer.ToString();
        }

        private string GenerateConsumablesCsv(IEnumerable<ConsumableDTO> consumables)
        {
            using var writer = new StringWriter();
            writer.WriteLine("ID,Printer,Name,Type,Current Level,Max Capacity,Unit,Status,Last Updated");

            foreach (var item in consumables)
            {
                writer.WriteLine($"\"{item.Id}\"," +
                    $"\"{EscapeCsv(item.PrinterName)}\"," +
                    $"\"{EscapeCsv(item.Name)}\"," +
                    $"\"{EscapeCsv(item.Type)}\"," +
                    $"{item.CurrentLevel}," +
                    $"{item.MaxCapacity}," +
                    $"\"{EscapeCsv(item.Unit)}\"," +
                    $"\"{EscapeCsv(item.Status)}\"," +
                    $"\"{item.LastUpdated:yyyy-MM-dd HH:mm:ss}\"");
            }

            return writer.ToString();
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\"", "\"\"");
        }
    }
}
