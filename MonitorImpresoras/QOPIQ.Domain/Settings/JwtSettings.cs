namespace QOPIQ.Domain.Settings
{
    public class JwtSettings
    {
        public string Secret { get; set; } = "ClaveSuperSegura_QOPIQ_2025";
        public string Issuer { get; set; } = "QOPIQ";
        public string Audience { get; set; } = "QOPIQ_Users";
        public int AccessTokenExpirationMinutes { get; set; } = 60;
    }
}
