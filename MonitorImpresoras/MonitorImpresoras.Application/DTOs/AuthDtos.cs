namespace MonitorImpresoras.Application.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime Expiration { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class RegisterRequestDto
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }

    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = default!;
    }
}
