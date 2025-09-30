using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonitorImpresoras.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Infrastructure.Data;
using MonitorImpresoras.Domain.Entities;
using System;

namespace MonitorImpresoras.Infrastructure.Services
{
    /// <summary>
    /// Implementación con EF Core para consultar impresoras desde la base de datos.
    /// </summary>
    public sealed class PrinterQueryService : IPrinterQueryService
    {
        private readonly ApplicationDbContext _db;

        public PrinterQueryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PrinterQueryDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Set<Printer>()
                .AsNoTracking()
                .Select(p => new PrinterQueryDto(
                    p.Id,
                    p.Name,
                    p.IpAddress,
                    p.Location
                ))
                .ToListAsync(ct);
        }

        public async Task<PrinterQueryDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Set<Printer>()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PrinterQueryDto(
                    p.Id,
                    p.Name,
                    p.IpAddress,
                    p.Location
                ))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(IEnumerable<PrinterQueryDto> Items, int Total)> SearchAsync(
            string? q,
            int page = 1,
            int pageSize = 20,
            string? orderBy = "Name",
            bool desc = false,
            CancellationToken ct = default)
        {
            var query = _db.Set<Printer>().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{q}%") ||
                    EF.Functions.ILike(p.IpAddress, $"%{q}%") ||
                    (p.Location != null && EF.Functions.ILike(p.Location, $"%{q}%"))
                );
            }

            // Ordering (minimal safe set)
            switch ((orderBy ?? "Name").Trim().ToLowerInvariant())
            {
                case "ipaddress":
                    query = desc ? query.OrderByDescending(p => p.IpAddress) : query.OrderBy(p => p.IpAddress);
                    break;
                case "location":
                    query = desc ? query.OrderByDescending(p => p.Location) : query.OrderBy(p => p.Location);
                    break;
                case "name":
                default:
                    query = desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                    break;
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PrinterQueryDto(
                    p.Id,
                    p.Name,
                    p.IpAddress,
                    p.Location
                ))
                .ToListAsync(ct);

            return (items, total);
        }
    }
}
