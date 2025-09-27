namespace MonitorImpresoras.Application.Interfaces.Services
{
    public interface ISnmpService
    {
        Task<string> GetAsync(string ipAddress, string oid);
        Task<Dictionary<string, string>> GetMultipleAsync(string ipAddress, string[] oids);
    }
}
