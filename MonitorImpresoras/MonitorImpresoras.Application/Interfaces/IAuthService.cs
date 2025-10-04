using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    /// <summary>
    /// Servicio de autenticación multi-tenant
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario en un tenant específico
        /// </summary>
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string tenantId);

        /// <summary>
        /// Registra un nuevo usuario en el tenant actual
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string tenantId);

        /// <summary>
        /// Renueva un token JWT
        /// </summary>
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshDto, string tenantId);

        /// <summary>
        /// Cierra sesión y revoca el refresh token
        /// </summary>
        Task<bool> LogoutAsync(string userId, string tenantId);

        /// <summary>
        /// Cambia la contraseña del usuario
        /// </summary>
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto, string tenantId);

        /// <summary>
        /// Valida si un token JWT es válido
        /// </summary>
        Task<bool> ValidateTokenAsync(string token, string tenantId);

        /// <summary>
        /// Obtiene información del usuario desde un token
        /// </summary>
        Task<UserInfoDto?> GetUserFromTokenAsync(string token);
    }
}
