using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Application.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IPrinterRepository _repository;

        public PrinterService(IPrinterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Printer?> GetPrinterByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Printer>> GetAllPrintersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Printer> CreatePrinterAsync(Printer printer)
        {
            await _repository.AddAsync(printer);
            await _repository.SaveChangesAsync();
            return printer;
        }

        public async Task<Printer?> UpdatePrinterAsync(Guid id, Printer updatedPrinter)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

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

            return existing;
        }

        public async Task<bool> DeletePrinterAsync(Guid id)
        {
            var printer = await _repository.GetByIdAsync(id);
            if (printer == null) return false;

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
