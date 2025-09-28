using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;
using Serilog;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string userId, string action, string entity, string? entityId = null, string? details = null, string? ipAddress = null, string? userAgent = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Details = details,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                // También loguear en Serilog para redundancia
                Log.Information("AUDIT: {Action} en {Entity} por usuario {UserId} - IP: {IpAddress}",
                    action, entity, userId, ipAddress);

                _logger.LogInformation("Auditoría registrada: {Action} en {Entity} por {UserId}",
                    action, entity, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al registrar auditoría: {Action} en {Entity} por {UserId}",
                    action, entity, userId);
                _logger.LogError(ex, "Error al registrar auditoría");
                // No relanzar la excepción para no interrumpir el flujo principal
            }
        }

        public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? userId = null, string? action = null, string? entity = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(a => a.UserId == userId);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action.Contains(action));

            if (!string.IsNullOrEmpty(entity))
                query = query.Where(a => a.Entity.Contains(entity));

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            query = query
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp);

            return await query.ToListAsync();
        }
    }
}
