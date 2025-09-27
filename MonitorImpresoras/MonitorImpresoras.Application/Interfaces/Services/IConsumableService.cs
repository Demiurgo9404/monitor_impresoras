namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface IConsumableService
    {
        Task CheckConsumablesAsync(Guid printerId);
        Task<bool> IsConsumableLowAsync(Guid printerId, string consumableType);
    }
}
