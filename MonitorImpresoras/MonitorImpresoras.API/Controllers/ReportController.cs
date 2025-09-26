using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportTemplateService _templateService;
        private readonly IReportExecutionService _executionService;
        private readonly IReportSchedulerService _schedulerService;

        public ReportController(
            IReportTemplateService templateService,
            IReportExecutionService executionService,
            IReportSchedulerService schedulerService)
        {
            _templateService = templateService;
            _executionService = executionService;
            _schedulerService = schedulerService;
        }

        /// <summary>
        /// Crea una nueva plantilla de reporte
        /// </summary>
        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] ReportTemplate template)
        {
            var createdTemplate = await _templateService.CreateTemplateAsync(template);
            return CreatedAtAction(nameof(GetTemplate), new { id = createdTemplate.Id }, createdTemplate);
        }

        /// <summary>
        /// Obtiene una plantilla por ID
        /// </summary>
        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();

            return Ok(template);
        }

        /// <summary>
        /// Obtiene todas las plantillas
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _templateService.GetAllTemplatesAsync();
            return Ok(templates);
        }

        /// <summary>
        /// Ejecuta un reporte
        /// </summary>
        [HttpPost("execute/{templateId}")]
        public async Task<IActionResult> ExecuteReport(int templateId)
        {
            var executionId = await _executionService.ExecuteReportAsync(templateId);
            return Ok(new { ExecutionId = executionId, Message = "Reporte ejecutado exitosamente" });
        }

        /// <summary>
        /// Prueba completa del sistema de reportes
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestReportSystem()
        {
            try
            {
                // 1. Crear una plantilla de prueba
                var template = new ReportTemplate
                {
                    Name = "Reporte de Prueba - Consumo de Tóner",
                    Description = "Reporte de prueba para verificar el funcionamiento",
                    ReportType = Domain.Enums.ReportType.PrinterUsage,
                    Format = Domain.Enums.ReportFormat.PDF,
                    IsActive = true,
                    CreatedBy = "system"
                };

                var createdTemplate = await _templateService.CreateTemplateAsync(template);

                // 2. Ejecutar el reporte
                var executionId = await _executionService.ExecuteReportAsync(createdTemplate.Id);

                // 3. Programar un reporte para dentro de 1 hora
                var scheduleTime = DateTime.UtcNow.AddHours(1);
                await _schedulerService.ScheduleReportAsync(createdTemplate.Id, scheduleTime);

                return Ok(new
                {
                    Message = "Prueba completada exitosamente",
                    TemplateId = createdTemplate.Id,
                    ExecutionId = executionId,
                    ScheduledFor = scheduleTime
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
