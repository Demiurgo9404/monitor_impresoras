namespace QOPIQ.Infrastructure.Configuration
{
    public class SnmpOptions
    {
        public string Community { get; set; } = "public";
        public int Timeout { get; set; } = 3000;
        public int Retries { get; set; } = 2;
    }
}
