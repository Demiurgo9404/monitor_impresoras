namespace QOPIQ.Infrastructure.Configuration
{
    /// <summary>
    /// Configuración SNMP v3 segura para monitoreo de impresoras
    /// </summary>
    public class SnmpOptions
    {
        public string Version { get; set; } = "V3";
        public string Community { get; set; } = "public";
        public int Timeout { get; set; } = 2000;
        public int Retries { get; set; } = 3;
        public string[] AllowedIPs { get; set; } = Array.Empty<string>();
        public SnmpV3Options V3 { get; set; } = new();
    }

    /// <summary>
    /// Configuración específica para SNMP v3
    /// </summary>
    public class SnmpV3Options
    {
        public string UserName { get; set; } = "qopiq_snmp_user";
        public string AuthProtocol { get; set; } = "SHA";
        public string AuthKey { get; set; } = "AuthKey123!";
        public string PrivacyProtocol { get; set; } = "AES";
        public string PrivacyKey { get; set; } = "PrivKey456!";
    }
}
