namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para solicitud de inicio de sesión
    /// </summary>
    /// <example>
    /// {
    ///   "username": "admin@monitorimpresoras.com",
    ///   "password": "Admin123!"
    /// }
    /// </example>
    public class LoginRequestDto
    {
        /// <summary>
        /// Nombre de usuario o correo electrónico
        /// </summary>
        /// <example>admin@monitorimpresoras.com</example>
        public string Username { get; set; } = default!;

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <example>Admin123!</example>
        public string Password { get; set; } = default!;
    }

    /// <summary>
    /// DTO para respuesta de inicio de sesión
    /// </summary>
    /// <example>
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIs...",
    ///   "refreshToken": "abc123xyz789...",
    ///   "expiration": "2024-12-28T16:30:00Z",
    ///   "roles": ["Admin"],
    ///   "userId": "admin-id"
    /// }
    /// </example>
    public class LoginResponseDto
    {
        /// <summary>
        /// Token de acceso JWT
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIs...</example>
        public string Token { get; set; } = default!;

        /// <summary>
        /// Token de refresco para renovar el token de acceso
        /// </summary>
        /// <example>abc123xyz789...</example>
        public string RefreshToken { get; set; } = default!;

        /// <summary>
        /// Fecha de expiración del token de acceso
        /// </summary>
        /// <example>2024-12-28T16:30:00Z</example>
        public DateTime Expiration { get; set; }

        /// <summary>
        /// Lista de roles del usuario
        /// </summary>
        /// <example>["Admin"]</example>
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// DTO para solicitud de registro de usuario
    /// </summary>
    /// <example>
    /// {
    ///   "username": "nuevo.usuario",
    ///   "email": "nuevo@empresa.com",
    ///   "password": "Password123!",
    ///   "firstName": "Nuevo",
    ///   "lastName": "Usuario"
    /// }
    /// </example>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Nombre de usuario único
        /// </summary>
        /// <example>nuevo.usuario</example>
        public string Username { get; set; } = default!;

        /// <summary>
        /// Correo electrónico único
        /// </summary>
        /// <example>nuevo@empresa.com</example>
        public string Email { get; set; } = default!;

        /// <summary>
        /// Contraseña del usuario (mínimo 6 caracteres)
        /// </summary>
        /// <example>Password123!</example>
        public string Password { get; set; } = default!;

        /// <summary>
        /// Nombre del usuario
        /// </summary>
        /// <example>Nuevo</example>
        public string FirstName { get; set; } = default!;

        /// <summary>
        /// Apellido del usuario
        /// </summary>
        /// <example>Usuario</example>
        public string LastName { get; set; } = default!;
    }

    /// <summary>
    /// DTO para solicitud de renovación de token
    /// </summary>
    /// <example>
    /// {
    ///   "refreshToken": "abc123xyz789..."
    /// }
    /// </example>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// Token de refresco válido
        /// </summary>
        /// <example>abc123xyz789...</example>
        public string RefreshToken { get; set; } = default!;
    }
}
