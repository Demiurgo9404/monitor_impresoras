using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResponseDTO> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDTO> GetCurrentUserAsync(string userId);
    }
}
