using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Application.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly List<Printer> _printers = new();

        public async Task<IEnumerable<PrinterDto>> GetAllAsync()
        {
            return await Task.FromResult(_printers.ConvertAll(p => new PrinterDto
            {
                Id = p.Id,
                Name = p.Name,
                Model = p.Model,
                IpAddress = p.IpAddress,
                Status = p.Status,
                Location = p.Location,
                LastUpdated = DateTime.UtcNow
            }));
        }

        public async Task<PrinterDto> GetPrinterByIdAsync(Guid id)
        {
            var printer = _printers.Find(p => p.Id == id);
            if (printer == null) return null;

            return await Task.FromResult(new PrinterDto
            {
                Id = printer.Id,
                Name = printer.Name,
                Model = printer.Model,
                IpAddress = printer.IpAddress,
                Status = printer.Status,
                Location = printer.Location,
                LastUpdated = DateTime.UtcNow
            });
        }

        public async Task AddPrinterAsync(PrinterCreateDto dto)
        {
            var printer = new Printer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Model = dto.Model,
                IpAddress = dto.IpAddress,
                Location = dto.Location,
                Status = "Online",
                CreatedAt = DateTime.UtcNow
            };

            _printers.Add(printer);
            await Task.CompletedTask;
        }

        public async Task UpdatePrinterAsync(Printer printer)
        {
            var existing = _printers.Find(p => p.Id == printer.Id);
            if (existing != null)
            {
                existing.Name = printer.Name;
                existing.Model = printer.Model;
                existing.IpAddress = printer.IpAddress;
                existing.Status = printer.Status;
                existing.Location = printer.Location;
            }
            await Task.CompletedTask;
        }

        public async Task DeletePrinterAsync(Guid id)
        {
            var printer = _printers.Find(p => p.Id == id);
            if (printer != null)
                _printers.Remove(printer);

            await Task.CompletedTask;
        }

        public async Task<PrinterStatusDto> GetPrinterStatusAsync(Guid id)
        {
            var printer = _printers.Find(p => p.Id == id);
            if (printer == null)
                return null;

            var status = new PrinterStatusDto
            {
                PrinterId = printer.Id,
                Status = printer.Status,
                LastChecked = DateTime.UtcNow
            };

            return await Task.FromResult(status);
        }

        public async Task<PrinterStatsDto> GetPrinterStatisticsAsync()
        {
            var total = _printers.Count;
            var online = _printers.FindAll(p => p.Status == "Online").Count;
            var offline = total - online;

            var stats = new PrinterStatsDto
            {
                TotalPrinters = total,
                OnlinePrinters = online,
                OfflinePrinters = offline
            };

            return await Task.FromResult(stats);
        }
    }
}
