using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Enums;

namespace QOPIQ.Application.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly List<Printer> _printers = new();
        private readonly IPrinterHubContext _printerHub;

        public PrinterService(IPrinterHubContext printerHub)
        {
            _printerHub = printerHub;
        }

        public async Task<IEnumerable<PrinterDto>> GetAllAsync()
        {
            return _printers.Select(p => new PrinterDto
            {
                Id = p.Id,
                Name = p.Name,
                Model = p.Model,
                IpAddress = p.IpAddress,
                Status = p.Status,
                Location = p.Location,
                UpdatedAt = DateTime.UtcNow
            });
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
                UpdatedAt = DateTime.UtcNow
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
                Status = PrinterStatus.Online,
                CreatedAt = DateTime.UtcNow
            };

            _printers.Add(printer);
            
            // Notificar a los clientes sobre la nueva impresora
            await _printerHub.SendPrinterUpdate(printer.Id.ToString(), PrinterStatus.Online.ToString());
            
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
                
                // Notificar a los clientes sobre la actualización
                await _printerHub.SendPrinterUpdate(existing.Id.ToString(), existing.Status.ToString());
            }
            await Task.CompletedTask;
        }

        public async Task DeletePrinterAsync(Guid id)
        {
            var printer = _printers.Find(p => p.Id == id);
            if (printer != null)
            {
                _printers.Remove(printer);
                // Notificar a los clientes sobre la eliminación
                await _printerHub.SendPrinterUpdate(printer.Id.ToString(), "Eliminada");
            }

            await Task.CompletedTask;
        }

        public async Task<PrinterStatusDto> GetPrinterStatusAsync(Guid id)
        {
            var printer = _printers.Find(p => p.Id == id);
            if (printer == null)
                return null;

            var status = new DTOs.PrinterStatusDto
            {
                PrinterId = printer.Id,
                Status = printer.Status,
                TonerLevel = printer.TonerLevel,
                TonerLevelPercentage = printer.TonerLevelPercentage,
                LastUpdate = DateTime.UtcNow,
                IsOnline = printer.Status == PrinterStatus.Online,
                Name = printer.Name,
                IpAddress = printer.IpAddress,
                StatusMessage = $"Estado actual: {printer.Status}",
                Message = ""
            };

            return await Task.FromResult(status);
        }

        public async Task<PrinterStatsDto> GetPrinterStatisticsAsync()
        {
            var total = _printers.Count;
            var online = _printers.FindAll(p => p.Status == PrinterStatus.Online).Count;
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
