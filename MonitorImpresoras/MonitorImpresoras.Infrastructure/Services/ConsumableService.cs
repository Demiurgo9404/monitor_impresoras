using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.DTOs;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class ConsumableService : IConsumableService
    {
        private readonly IConsumableRepository _consumableRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly ISignalRNotificationService _signalRService;

        public ConsumableService(
            IConsumableRepository consumableRepository,
            IPrinterRepository printerRepository,
            IAlertRepository alertRepository,
            ISignalRNotificationService signalRService)
        {
            _consumableRepository = consumableRepository;
            _printerRepository = printerRepository;
            _alertRepository = alertRepository;
            _signalRService = signalRService;
        }

        public async Task<ConsumableDTO> GetConsumableByIdAsync(Guid id)
        {
            var consumable = await _consumableRepository.GetByIdAsync(id);
            return consumable != null ? MapToConsumableDTO(consumable) : null;
        }

        public async Task<IEnumerable<ConsumableDTO>> GetConsumablesByFilterAsync(ConsumableFilterDTO filter)
        {
            // Implementation for filtering consumables
            var consumables = await _consumableRepository.GetAllAsync();
            return consumables.Select(MapToConsumableDTO);
        }

        public async Task<ConsumableDTO> UpdateConsumableLevelAsync(UpdateConsumableLevelDTO updateDto)
        {
            var consumable = await _consumableRepository.GetByIdAsync(updateDto.ConsumableId);
            if (consumable == null)
                return null;

            if (updateDto.ResetCounter)
            {
                consumable.CurrentLevel = consumable.MaxCapacity;
            }
            else if (updateDto.CurrentLevel.HasValue)
            {
                consumable.CurrentLevel = updateDto.CurrentLevel.Value;
            }

            consumable.LastUpdated = DateTime.UtcNow;
            consumable.Status = CalculateConsumableStatus(consumable);

            await _consumableRepository.UpdateAsync(consumable);

            var consumableResult = MapToConsumableDTO(consumable);

            // Check if consumable is low and send notification
            if (consumable.Status == "low" || consumable.Status == "critical")
            {
                await _signalRService.NotifyLowConsumableAlert(new ConsumableAlertDTO
                {
                    PrinterId = consumable.PrinterId,
                    PrinterName = consumable.Printer.Name,
                    ConsumableId = consumable.Id,
                    ConsumableName = consumable.Name,
                    Type = consumable.Type,
                    CurrentLevel = consumable.CurrentLevel,
                    MaxCapacity = consumable.MaxCapacity,
                    Status = consumable.Status,
                    IsCritical = consumable.Status == "critical"
                });
            }

            return consumableResult;
        }

        public async Task<IEnumerable<ConsumableDTO>> GetConsumablesByPrinterIdAsync(Guid printerId)
        {
            var consumables = await _consumableRepository.GetConsumablesByPrinterIdAsync(printerId);
            return consumables.Select(MapToConsumableDTO);
        }

        public async Task<ConsumableStatsDTO> GetConsumableStatsAsync()
        {
            var consumables = await _consumableRepository.GetAllAsync();
            var printers = await _printerRepository.GetAllAsync();

            return new ConsumableStatsDTO
            {
                TotalConsumables = consumables.Count(),
                LowConsumables = consumables.Count(c => c.Status == "low"),
                CriticalConsumables = consumables.Count(c => c.Status == "critical"),
                PrintersWithLowConsumables = printers.Count(p =>
                    p.Consumables.Any(c => c.Status == "low" || c.Status == "critical")),
                ConsumablesByType = consumables.GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => g.Count()),
                CriticalItems = consumables.Where(c => c.Status == "critical")
                    .Select(MapToConsumableDTO)
                    .ToList()
            };
        }

        public async Task CheckAndCreateLowConsumableAlertsAsync()
        {
            var consumables = await _consumableRepository.GetAllAsync();

            foreach (var consumable in consumables.Where(c => c.Status == "low" || c.Status == "critical"))
            {
                // Check if there's already an active alert for this consumable
                var existingAlerts = await _alertRepository.GetByConsumableIdAsync(consumable.Id);
                var activeAlert = existingAlerts.FirstOrDefault(a => a.Status == "Active");

                if (activeAlert == null)
                {
                    var alert = new Alert
                    {
                        Type = consumable.Status == "critical" ? "CriticalConsumable" : "LowConsumable",
                        Title = $"{consumable.Printer.Name} - {consumable.Name}",
                        Message = $"El consumible {consumable.Name} de la impresora {consumable.Printer.Name} " +
                                 $"está {consumable.Status} ({consumable.CurrentLevel}/{consumable.MaxCapacity} {consumable.Unit})",
                        PrinterId = consumable.PrinterId,
                        Source = "System",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        Metadata = $"ConsumableId:{consumable.Id}"
                    };

                    await _alertRepository.AddAsync(alert);

                    // Send SignalR notification
                    await _signalRService.NotifyLowConsumableAlert(new ConsumableAlertDTO
                    {
                        PrinterId = consumable.PrinterId,
                        PrinterName = consumable.Printer.Name,
                        ConsumableId = consumable.Id,
                        ConsumableName = consumable.Name,
                        Type = consumable.Type,
                        CurrentLevel = consumable.CurrentLevel,
                        MaxCapacity = consumable.MaxCapacity,
                        Status = consumable.Status,
                        IsCritical = consumable.Status == "critical"
                    });
                }
            }
        }

        private ConsumableDTO MapToConsumableDTO(PrinterConsumable consumable)
        {
            return new ConsumableDTO
            {
                Id = consumable.Id,
                PrinterId = consumable.PrinterId,
                PrinterName = consumable.Printer.Name,
                Name = consumable.Name,
                Type = consumable.Type,
                PartNumber = consumable.PartNumber,
                MaxCapacity = consumable.MaxCapacity,
                CurrentLevel = consumable.CurrentLevel,
                Unit = consumable.Unit,
                WarningLevel = consumable.WarningLevel,
                CriticalLevel = consumable.CriticalLevel,
                LastUpdated = consumable.LastUpdated,
                Status = consumable.Status,
                RemainingPages = consumable.RemainingPages
            };
        }

        private string CalculateConsumableStatus(PrinterConsumable consumable)
        {
            if (!consumable.CurrentLevel.HasValue || !consumable.MaxCapacity.HasValue)
                return "unknown";

            var percentage = (double)consumable.CurrentLevel.Value / consumable.MaxCapacity.Value * 100;

            if (consumable.CriticalLevel.HasValue && consumable.CurrentLevel.Value <= consumable.CriticalLevel.Value)
                return "critical";
            else if (consumable.WarningLevel.HasValue && consumable.CurrentLevel.Value <= consumable.WarningLevel.Value)
                return "low";
            else if (percentage < 10)
                return "low";
            else
                return "normal";
        }
    }
}
