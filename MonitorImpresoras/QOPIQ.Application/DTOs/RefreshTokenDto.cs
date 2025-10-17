using System;

namespace QOPIQ.Application.DTOs
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
    }
}
