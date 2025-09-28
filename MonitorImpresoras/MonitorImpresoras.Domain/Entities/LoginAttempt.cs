using System;

namespace MonitorImpresoras.Domain.Entities
{
    public class LoginAttempt
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? IpAddress { get; set; }
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; }
        public string? UserAgent { get; set; }
        public string? FailureReason { get; set; }

        // Navegación
        public virtual User? User { get; set; }
    }
}
