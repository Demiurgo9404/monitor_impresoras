using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using System.Text.Json;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para gestión de auditoría extendida
    /// </summary>
    public class ExtendedAuditService : IExtendedAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExtendedAuditService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExtendedAuditService(
            ApplicationDbContext context,
            ILogger<ExtendedAuditService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogEventAsync(SystemEvent systemEvent)
        {
            try
            {
                // Enriquecer el evento con información del contexto HTTP
                EnrichEventFromHttpContext(systemEvent);

                _context.SystemEvents.Add(systemEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Evento de auditoría registrado: {EventType} - {Title}", systemEvent.EventType, systemEvent.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar evento de auditoría");
                throw;
            }
        }

        public async Task<IEnumerable<SystemEventDto>> GetSystemEventsAsync(
            string? eventType = null,
            string? category = null,
            string? severity = null,
            string? userId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50)
        {
            var query = _context.SystemEvents.AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(eventType))
            {
                query = query.Where(e => e.EventType.Contains(eventType));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(e => e.Category.Contains(category));
            }

            if (!string.IsNullOrEmpty(severity))
            {
                query = query.Where(e => e.Severity == severity);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(e => e.UserId == userId);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(e => e.TimestampUtc >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(e => e.TimestampUtc <= dateTo.Value);
            }

            var events = await query
                .Include(e => e.User)
                .OrderByDescending(e => e.TimestampUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new SystemEventDto
                {
                    Id = e.Id,
                    EventType = e.EventType,
                    Category = e.Category,
                    Severity = e.Severity,
                    Title = e.Title,
                    Description = e.Description,
                    EventData = e.EventData,
                    UserId = e.UserId,
                    UserName = e.User != null ? e.User.UserName : null,
                    IpAddress = e.IpAddress,
                    UserAgent = e.UserAgent,
                    SessionId = e.SessionId,
                    RequestId = e.RequestId,
                    Endpoint = e.Endpoint,
                    HttpMethod = e.HttpMethod,
                    HttpStatusCode = e.HttpStatusCode,
                    ExecutionTimeMs = e.ExecutionTimeMs,
                    IsSuccess = e.IsSuccess,
                    ErrorMessage = e.ErrorMessage,
                    TimestampUtc = e.TimestampUtc,
                    CreatedAtUtc = e.CreatedAtUtc
                })
                .ToListAsync();

            return events;
        }

        public async Task<SystemEventStatisticsDto> GetSystemEventStatisticsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.SystemEvents.AsQueryable();

            if (dateFrom.HasValue)
            {
                query = query.Where(e => e.TimestampUtc >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(e => e.TimestampUtc <= dateTo.Value);
            }

            var statistics = await query.GroupBy(e => 1)
                .Select(g => new SystemEventStatisticsDto
                {
                    TotalEvents = g.Count(),
                    EventsByType = g.GroupBy(e => e.EventType)
                        .ToDictionary(eg => eg.Key, eg => eg.Count()),
                    EventsByCategory = g.GroupBy(e => e.Category)
                        .ToDictionary(eg => eg.Key, eg => eg.Count()),
                    EventsBySeverity = g.GroupBy(e => e.Severity)
                        .ToDictionary(eg => eg.Key, eg => eg.Count()),
                    SuccessfulEvents = g.Count(e => e.IsSuccess),
                    FailedEvents = g.Count(e => !e.IsSuccess),
                    AverageExecutionTimeMs = g.Where(e => e.ExecutionTimeMs.HasValue)
                        .Average(e => e.ExecutionTimeMs.Value),
                    EventsByUser = g.Where(e => e.UserId != null)
                        .GroupBy(e => e.UserId!)
                        .ToDictionary(eg => eg.Key, eg => eg.Count()),
                    MostCommonErrors = g.Where(e => !e.IsSuccess && !string.IsNullOrEmpty(e.ErrorMessage))
                        .GroupBy(e => e.ErrorMessage!)
                        .OrderByDescending(eg => eg.Count())
                        .Take(10)
                        .ToDictionary(eg => eg.Key, eg => eg.Count())
                })
                .FirstOrDefaultAsync();

            return statistics ?? new SystemEventStatisticsDto();
        }

        public async Task<int> CleanupOldEventsAsync(int retentionDays = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var deletedCount = await _context.SystemEvents
                .Where(e => e.TimestampUtc < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Eliminados {DeletedCount} eventos antiguos de auditoría", deletedCount);

            return deletedCount;
        }

        private void EnrichEventFromHttpContext(SystemEvent systemEvent)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                // Información del usuario autenticado
                var userId = httpContext.User.FindFirst("sub")?.Value ??
                           httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    systemEvent.UserId = userId;
                }

                // Información de la solicitud HTTP
                systemEvent.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                systemEvent.UserAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
                systemEvent.RequestId = httpContext.TraceIdentifier;
                systemEvent.Endpoint = $"{httpContext.Request.Method} {httpContext.Request.Path}";
                systemEvent.HttpMethod = httpContext.Request.Method;
                systemEvent.HttpStatusCode = httpContext.Response.StatusCode;

                // Información del entorno
                systemEvent.EnvironmentInfo = $"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}";
                systemEvent.ApplicationVersion = typeof(ExtendedAuditService).Assembly.GetName().Version?.ToString();
                systemEvent.ServerName = Environment.MachineName;
            }
        }

        // Métodos específicos para eventos comunes
        public async Task LogReportEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string? userId = null, bool isSuccess = true,
            string? errorMessage = null, long? executionTimeMs = null)
        {
            var systemEvent = new SystemEvent
            {
                EventType = eventType,
                Category = "reports",
                Severity = isSuccess ? "Info" : "Error",
                Title = title,
                Description = description,
                EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                UserId = userId,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                ExecutionTimeMs = executionTimeMs,
                TimestampUtc = DateTime.UtcNow
            };

            await LogEventAsync(systemEvent);
        }

        public async Task LogEmailEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string? userId = null, bool isSuccess = true,
            string? errorMessage = null)
        {
            var systemEvent = new SystemEvent
            {
                EventType = eventType,
                Category = "emails",
                Severity = isSuccess ? "Info" : "Error",
                Title = title,
                Description = description,
                EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                UserId = userId,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                TimestampUtc = DateTime.UtcNow
            };

            await LogEventAsync(systemEvent);
        }

        public async Task LogSecurityEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string? userId = null, bool isSuccess = true,
            string? errorMessage = null, string severity = "Info")
        {
            var systemEvent = new SystemEvent
            {
                EventType = eventType,
                Category = "security",
                Severity = severity,
                Title = title,
                Description = description,
                EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                UserId = userId,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                TimestampUtc = DateTime.UtcNow
            };

            await LogEventAsync(systemEvent);
        }

        public async Task LogSystemEventAsync(string eventType, string title, string? description = null,
            Dictionary<string, object>? eventData = null, string severity = "Info", bool isSuccess = true)
        {
            var systemEvent = new SystemEvent
            {
                EventType = eventType,
                Category = "system",
                Severity = severity,
                Title = title,
                Description = description,
                EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                IsSuccess = isSuccess,
                TimestampUtc = DateTime.UtcNow
            };

            await LogEventAsync(systemEvent);
        }
    }
}
