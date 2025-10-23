namespace QOPIQ.Domain.Settings
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = "super_secret_key_1234567890_1234567890_1234567890_12";
        public string Issuer { get; set; } = "QOPIQ.API";
        public string Audience { get; set; } = "QOPIQ.Client";
        public int ExpirationInMinutes { get; set; } = 60;
        public int RefreshTokenExpirationInDays { get; set; } = 7;
        
        // Propiedades legacy para compatibilidad
        public string Secret => SecretKey;
        public int AccessTokenExpirationMinutes => ExpirationInMinutes;
    }
}
