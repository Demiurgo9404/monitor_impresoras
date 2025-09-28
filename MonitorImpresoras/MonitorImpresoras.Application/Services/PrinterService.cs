using System.Security.Claims;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using Serilog;

namespace MonitorImpresoras.Application.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IPrinterRepository _repository;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PrinterService(IPrinterRepository repository, IAuditService auditService, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Printer?> GetPrinterByIdAsync(Guid id)
        {
            var printer = await _repository.GetByIdAsync(id);

            if (printer != null)
            {
                var userId = GetCurrentUserId();
                await _auditService.LogAsync(userId, "PRINTER_VIEWED", "Printer", id.ToString(),
                    $"Impresora consultada: {printer.Name}");

                Log.Information("Usuario {UserId} consultó impresora {PrinterId}: {PrinterName}",
                    userId, id, printer.Name);
            }

            return printer;
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
        {
            var printers = await _repository.GetAllAsync();

            var userId = GetCurrentUserId();
            await _auditService.LogAsync(userId, "PRINTERS_LISTED", "Printer", null,
                $"Listado de {printers.Count()} impresoras consultado");

            Log.Information("Usuario {UserId} consultó listado de impresoras ({Count} impresoras)",
                userId, printers.Count());

            return printers;
        }

        public async Task<Printer> CreatePrinterAsync(Printer printer)
        {
            await _repository.AddAsync(printer);
            await _repository.SaveChangesAsync();

            var userId = GetCurrentUserId();
            await _auditService.LogAsync(userId, "PRINTER_CREATED", "Printer", printer.Id.ToString(),
                $"Nueva impresora creada: {printer.Name} ({printer.IpAddress})");

            Log.Information("Nueva impresora creada por usuario {UserId}: {PrinterName} ({IpAddress})",
                userId, printer.Name, printer.IpAddress);

            return printer;
        }

        public async Task<Printer?> UpdatePrinterAsync(Guid id, Printer updatedPrinter)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            var oldName = existing.Name;
            var oldIp = existing.IpAddress;

            existing.Name = updatedPrinter.Name;
            existing.IpAddress = updatedPrinter.IpAddress;
            existing.Location = updatedPrinter.Location;
            existing.Model = updatedPrinter.Model;
            existing.Status = updatedPrinter.Status;
            existing.IsOnline = updatedPrinter.IsOnline;
            existing.IsLocalPrinter = updatedPrinter.IsLocalPrinter;
            existing.SerialNumber = updatedPrinter.SerialNumber;
            existing.CommunityString = updatedPrinter.CommunityString;
            existing.SnmpPort = updatedPrinter.SnmpPort;
            existing.PageCount = updatedPrinter.PageCount;
            existing.LastMaintenance = updatedPrinter.LastMaintenance;
            existing.MaintenanceIntervalDays = updatedPrinter.MaintenanceIntervalDays;
            existing.Notes = updatedPrinter.Notes;
            existing.LastError = updatedPrinter.LastError;
            existing.LastChecked = updatedPrinter.LastChecked;
            existing.LastSeen = updatedPrinter.LastSeen;

            await _repository.UpdateAsync(existing);
            await _repository.SaveChangesAsync();

            var userId = GetCurrentUserId();
            await _auditService.LogAsync(userId, "PRINTER_UPDATED", "Printer", id.ToString(),
                $"Impresora actualizada: {oldName} -> {existing.Name}, IP: {oldIp} -> {existing.IpAddress}");

            Log.Information("Impresora actualizada por usuario {UserId}: {PrinterName} ({IpAddress})",
                userId, existing.Name, existing.IpAddress);

            return existing;
        }

        public async Task<bool> DeletePrinterAsync(Guid id)
        {
            var printer = await _repository.GetByIdAsync(id);
            if (printer == null) return false;

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            var userId = GetCurrentUserId();
            await _auditService.LogAsync(userId, "PRINTER_DELETED", "Printer", id.ToString(),
                $"Impresora eliminada: {printer.Name} ({printer.IpAddress})");

            Log.Warning("Impresora eliminada por usuario {UserId}: {PrinterName} ({IpAddress})",
                userId, printer.Name, printer.IpAddress);

            return true;
        }

        private string GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
        }
    }
}
