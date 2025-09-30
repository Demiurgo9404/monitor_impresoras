using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Common;

namespace MonitorImpresoras.Application.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public TelemetryService(IApplicationDbContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<PrinterTelemetry> GetLatestTelemetryAsync(int printerId)
        {
            return await _context.PrinterTelemetries
                .Where(t => t.PrinterId == printerId && t.TenantId == _currentUser.TenantId)
                .OrderByDescending(t => t.TimestampUtc)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SaveTelemetryAsync(PrinterTelemetry telemetry)
        {
            telemetry.TenantId = _currentUser.TenantId;
            _context.PrinterTelemetries.Add(telemetry);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<PrinterTelemetry>> GetHistoricalDataAsync(int printerId, DateTime from, DateTime to)
        {
            return await _context.PrinterTelemetries
                .Where(t => t.PrinterId == printerId 
                         && t.TenantId == _currentUser.TenantId
                         && t.TimestampUtc >= from 
                         && t.TimestampUtc <= to)
                .OrderBy(t => t.TimestampUtc)
                .ToListAsync();
        }
    }
}
