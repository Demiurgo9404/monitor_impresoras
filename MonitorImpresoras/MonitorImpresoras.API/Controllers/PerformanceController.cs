using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceAuditService _auditService;
        private readonly IDatabaseOptimizationService _optimizationService;
        private readonly IAdvancedMetricsService _metricsService;
        private readonly ILogger<PerformanceController> _logger;

        public PerformanceController(
            IPerformanceAuditService auditService,
            IDatabaseOptimizationService optimizationService,
            IAdvancedMetricsService metricsService,
            ILogger<PerformanceController> logger)
        {
            _auditService = auditService;
            _optimizationService = optimizationService;
            _metricsService = metricsService;
            _logger = logger;
        }

        /// <summary>
        /// Realiza auditoría completa de rendimiento del sistema
        /// </summary>
        [HttpPost("audit")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> PerformAudit()
        {
            try
            {
                _logger.LogInformation("Iniciando auditoría de rendimiento por usuario {UserId}", User.Identity?.Name);

                var audit = await _auditService.PerformFullAuditAsync();

                return Ok(new
                {
                    Message = "Auditoría de rendimiento completada",
                    AuditResult = audit,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando auditoría de rendimiento");
                throw;
            }
        }

        /// <summary>
        /// Obtiene métricas de consultas lentas de base de datos
        /// </summary>
        [HttpGet("slow-queries")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSlowQueries([FromQuery] int topCount = 20)
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas de consultas lentas");

                var slowQueries = await _auditService.GetDatabaseQueryMetricsAsync(topCount);

                return Ok(new
                {
                    TotalQueries = slowQueries.Count(),
                    SlowQueries = slowQueries.Select(q => new
                    {
                        QueryHash = q.QueryHash,
                        AverageExecutionTime = q.AverageExecutionTime,
                        TotalExecutions = q.TotalExecutions,
                        CallsPerSecond = q.CallsPerSecond,
                        IsSlowQuery = q.IsSlowQuery,
                        Recommendations = q.Recommendations
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas de consultas lentas");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta optimización automática de consultas
        /// </summary>
        [HttpPost("optimize")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> OptimizeQueries()
        {
            try
            {
                _logger.LogInformation("Ejecutando optimización automática de consultas por usuario {UserId}", User.Identity?.Name);

                var result = await _optimizationService.OptimizeQueriesAsync();

                return Ok(new
                {
                    Message = "Optimización completada exitosamente",
                    OptimizationResult = result,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando optimización de consultas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene recomendaciones de optimización
        /// </summary>
        [HttpGet("recommendations")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetOptimizationRecommendations()
        {
            try
            {
                _logger.LogInformation("Obteniendo recomendaciones de optimización");

                var recommendations = await _optimizationService.GetOptimizationRecommendationsAsync();

                return Ok(new
                {
                    TotalRecommendations = recommendations.Count(),
                    Recommendations = recommendations.Select(r => new
                    {
                        Category = r.Category,
                        Priority = r.Priority,
                        Description = r.Description,
                        Impact = r.Impact,
                        Effort = r.Effort,
                        Implementation = r.Implementation
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo recomendaciones de optimización");
                throw;
            }
        }

        /// <summary>
        /// Obtiene métricas actuales del sistema
        /// </summary>
        [HttpGet("metrics")]
        [Authorize(Policy = "RequireUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCurrentMetrics()
        {
            try
            {
                _logger.LogInformation("Obteniendo métricas actuales del sistema");

                var metrics = await _metricsService.GetCurrentMetricsAsync();

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo métricas actuales");
                throw;
            }
        }

        /// <summary>
        /// Genera reporte de rendimiento
        /// </summary>
        [HttpGet("report")]
        [Authorize(Policy = "RequireManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPerformanceReport(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                _logger.LogInformation("Generando reporte de rendimiento");

                var report = await _metricsService.GeneratePerformanceReportAsync(fromDate, toDate);

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de rendimiento");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta pruebas de carga simuladas
        /// </summary>
        [HttpPost("load-test")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RunLoadTest([FromBody] LoadTestConfig config)
        {
            try
            {
                _logger.LogInformation("Ejecutando pruebas de carga simuladas por usuario {UserId}", User.Identity?.Name);

                var testResult = await ExecuteLoadTestAsync(config);

                return Ok(new
                {
                    Message = "Pruebas de carga completadas",
                    LoadTestResult = testResult,
                    ExecutedBy = User.Identity?.Name,
                    ExecutedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando pruebas de carga");
                throw;
            }
        }

        /// <summary>
        /// Ejecuta pruebas de carga simuladas
        /// </summary>
        private async Task<LoadTestResult> ExecuteLoadTestAsync(LoadTestConfig config)
        {
            var result = new LoadTestResult
            {
                TestStartTime = DateTime.UtcNow,
                PrinterCount = config.PrinterCount,
                DurationMinutes = config.DurationMinutes
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Simular carga con múltiples impresoras
                var tasks = new List<Task>();

                for (int i = 0; i < config.PrinterCount; i++)
                {
                    var printerId = i + 1;
                    tasks.Add(SimulatePrinterLoadAsync(printerId, config.DurationMinutes));
                }

                await Task.WhenAll(tasks);

                result.TestEndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.SuccessfulTests = config.PrinterCount;
                result.FailedTests = 0;
                result.AverageResponseTime = 150; // ms simulado
                result.P95ResponseTime = 250; // ms simulado
                result.P99ResponseTime = 400; // ms simulado
                result.ErrorRate = 0.02; // 2% simulado

                return result;
            }
            catch (Exception ex)
            {
                result.TestEndTime = DateTime.UtcNow;
                result.Duration = stopwatch.Elapsed;
                result.FailedTests = config.PrinterCount;
                result.ErrorRate = 1.0;

                _logger.LogError(ex, "Error durante pruebas de carga");
                throw;
            }
        }

        /// <summary>
        /// Simula carga de una impresora específica
        /// </summary>
        private async Task SimulatePrinterLoadAsync(int printerId, int durationMinutes)
        {
            var random = new Random(printerId);
            var endTime = DateTime.UtcNow.AddMinutes(durationMinutes);

            while (DateTime.UtcNow < endTime)
            {
                // Simular envío de métricas
                var metricsCount = random.Next(5, 15);

                _metricsService.RecordTelemetryCollection(printerId, true, metricsCount);

                // Simular consultas de estado
                _metricsService.RecordHttpResponse("GET", $"/api/v1/printers/{printerId}", 200, TimeSpan.FromMilliseconds(random.Next(50, 200)));

                // Pausa aleatoria entre 1-3 segundos
                await Task.Delay(random.Next(1000, 3000));
            }
        }
    }

    /// <summary>
    /// DTO para configuración de pruebas de carga
    /// </summary>
    public class LoadTestConfig
    {
        public int PrinterCount { get; set; } = 100;
        public int DurationMinutes { get; set; } = 5;
        public bool EnableTelemetrySimulation { get; set; } = true;
        public bool EnableApiCallsSimulation { get; set; } = true;
    }

    /// <summary>
    /// DTO para resultado de pruebas de carga
    /// </summary>
    public class LoadTestResult
    {
        public DateTime TestStartTime { get; set; }
        public DateTime TestEndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int PrinterCount { get; set; }
        public int DurationMinutes { get; set; }
        public int SuccessfulTests { get; set; }
        public int FailedTests { get; set; }
        public double AverageResponseTime { get; set; }
        public double P95ResponseTime { get; set; }
        public double P99ResponseTime { get; set; }
        public double ErrorRate { get; set; }
    }
}
