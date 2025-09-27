using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MonitorImpresoras.Application.Interfaces.Services;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Domain.Interfaces;
using MonitorImpresoras.Domain.Enums;

namespace MonitorImpresoras.Application.Services
{
    public class ConsumableService : IConsumableService
    {
        private readonly IConsumableRepository _consumableRepository;
        private readonly IPrinterRepository _printerRepository;
        private readonly ILogger<ConsumableService> _logger;

        public ConsumableService(
            IConsumableRepository consumableRepository,
            IPrinterRepository printerRepository,
            ILogger<ConsumableService> logger)
        {
            _consumableRepository = consumableRepository ?? throw new ArgumentNullException(nameof(consumableRepository));
            _printerRepository = printerRepository ?? throw new ArgumentNullException(nameof(printerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task CheckConsumablesAsync(Guid printerId)
        {
            try
            {
                var printer = await _printerRepository.GetByIdAsync(printerId);
                if (printer == null)
                {
                    throw new KeyNotFoundException($"Printer with ID {printerId} not found");
                }

                var consumables = await _consumableRepository.GetConsumablesByPrinterIdAsync(printerId);
                var lowConsumables = new List<PrinterConsumable>();

                foreach (var consumable in consumables)
                {
                    if (consumable.CurrentLevel <= (consumable.WarningLevel ?? 100))
                    {
                        lowConsumables.Add(consumable);
                        _logger.LogWarning(
                            "Low consumable detected: {ConsumableType} for printer {PrinterName}. Level: {CurrentLevel}%",
                            consumable.Type, printer.Name, consumable.CurrentLevel);
                    }
                }

                if (lowConsumables.Any())
                {
                    _logger.LogInformation(
                        "Found {Count} low consumables for printer {PrinterName}",
                        lowConsumables.Count, printer.Name);
                }
                else
                {
                    _logger.LogInformation("All consumables are at normal levels for printer {PrinterName}", printer.Name);
                }

                // Aquí podrías implementar lógica para crear alertas automáticas
                // await _alertService.CreateLowConsumableAlertAsync(lowConsumables);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking consumables for printer {PrinterId}", printerId);
                throw new ApplicationException($"Failed to check consumables for printer {printerId}", ex);
            }
        }

        public async Task<bool> IsConsumableLowAsync(Guid printerId, string consumableType)
        {
            try
            {
                // Try to parse the string to ConsumableType enum
                if (!Enum.TryParse<ConsumableType>(consumableType, true, out var consumableTypeEnum))
                {
                    _logger.LogWarning("Invalid consumable type: {ConsumableType} for printer {PrinterId}", consumableType, printerId);
                    return false;
                }

                var consumables = await _consumableRepository.GetConsumablesByPrinterIdAsync(printerId);
                var consumable = consumables.FirstOrDefault(c => c.Type == consumableTypeEnum);

                if (consumable == null)
                {
                    _logger.LogWarning("Consumable type {ConsumableType} not found for printer {PrinterId}", consumableType, printerId);
                    return false;
                }

                return consumable.CurrentLevel <= (consumable.WarningLevel ?? 100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if consumable {ConsumableType} is low for printer {PrinterId}", consumableType, printerId);
                return false;
            }
        }
    }
}
