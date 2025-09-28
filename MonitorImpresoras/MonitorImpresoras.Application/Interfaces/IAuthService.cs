using System.Threading.Tasks;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent);
        Task<bool> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress);
        Task<bool> LogoutAsync(string userId);
    }
}
