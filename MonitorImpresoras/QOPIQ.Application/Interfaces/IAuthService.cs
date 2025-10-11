using System.Threading.Tasks;
using QOPIQ.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace QOPIQ.Application.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario con email y contraseña
        /// </summary>
        Task<UserDto> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        Task<IdentityResult> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// Genera un nuevo token de actualización para un usuario
        /// </summary>
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId);

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        Task<UserDto> GetUserByIdAsync(string userId);

        /// <summary>
        /// Revoca un token de actualización
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string token);
    }

    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }

    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
