using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    public class PrintJobsController : BaseApiController
    {
        private readonly IPrintJobService _printJobService;
        private readonly ILogger<PrintJobsController> _logger;
        private readonly IMapper _mapper;

        public PrintJobsController(
            IPrintJobService printJobService,
            ILogger<PrintJobsController> logger,
            IMapper mapper)
        {
            _printJobService = printJobService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<IEnumerable<PrintJobDTO>>> GetPrintJobs(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? printerId = null,
            [FromQuery] string userId = null,
            [FromQuery] bool? isColor = null,
            [FromQuery] string jobStatus = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var filter = new PrintJobFilterDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                PrinterId = printerId,
                UserId = userId ?? UserId,
                IsColor = isColor,
                JobStatus = jobStatus,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var jobs = await _printJobService.GetPrintJobsByFilterAsync(filter);
            // Note: For pagination, you might want to update this to use HandlePagedResult
            return HandleResult(jobs);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<PrintJobDTO>> GetPrintJob(int id)
        {
            var job = await _printJobService.GetPrintJobByIdAsync(id);
            
            // Ensure users can only access their own jobs unless they're admin/technician
            if (job.UserId != UserId && !User.IsInRole("Admin") && !User.IsInRole("Technician"))
                return Forbid();
                
            return HandleResult(job);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<PrintJobDTO>> CreatePrintJob(CreatePrintJobDTO printJobDto)
        {
            var result = await _printJobService.CreatePrintJobAsync(printJobDto, UserId);
            return CreatedAtAction(nameof(GetPrintJob), new { id = result.Id }, result);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<IActionResult> CancelPrintJob(int id)
        {
            var job = await _printJobService.GetPrintJobByIdAsync(id);
            
            // Ensure users can only cancel their own jobs unless they're admin/technician
            if (job.UserId != UserId && !User.IsInRole("Admin") && !User.IsInRole("Technician"))
                return Forbid();
                
            var result = await _printJobService.CancelPrintJobAsync(id, UserId);
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Technician,User")]
        public async Task<ActionResult<PrintJobSummaryDTO>> GetPrintJobSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? printerId = null)
        {
            var filter = new PrintJobFilterDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                PrinterId = printerId
            };

            var summary = await _printJobService.GetPrintJobSummaryAsync(filter);
            return HandleResult(summary);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult<IEnumerable<PrintJobDTO>>> GetUserPrintJobs(
            string userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var filter = new PrintJobFilterDTO
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };

            var jobs = await _printJobService.GetPrintJobsByFilterAsync(filter);
            return HandleResult(jobs);
        }
    }
}
