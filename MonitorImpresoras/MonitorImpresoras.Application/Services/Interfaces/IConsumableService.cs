namespace MonitorImpresoras.Application.Services.Interfaces
{
    public interface IConsumableService
    {
        Task CheckConsumablesAsync(Guid printerId);
    }
}
