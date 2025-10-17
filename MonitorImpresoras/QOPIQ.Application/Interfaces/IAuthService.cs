using System;
using System.Threading.Tasks;
using QOPIQ.Application.DTOs;

namespace QOPIQ.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginDto loginDto, string ipAddress);
        Task<UserDto> RefreshTokenAsync(string token, string ipAddress);
        Task<bool> LogoutAsync(string token);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
    }
}
