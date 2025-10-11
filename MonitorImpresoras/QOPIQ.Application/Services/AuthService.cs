using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using QOPIQ.Domain.Entities;
using QOPIQ.Application.Interfaces;

namespace QOPIQ.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<User> _signInManager;

        public AuthService(SignInManager<User> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> SignInAsync(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            return result.Succeeded;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                return _jwtService.ValidateToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ValidateTokenAsync");
                return false;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                // Invalidar el refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                // Cerrar sesión
                await _signInManager.SignOutAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en LogoutAsync");
                return false;
            }
        }

        // Métodos auxiliares
    }
}
