namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface ITenantServiceSimple
    {
        string GetTenantId();
        string GetConnectionString();
    }
}
