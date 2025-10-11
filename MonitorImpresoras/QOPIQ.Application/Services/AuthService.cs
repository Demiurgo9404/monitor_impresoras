using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Entities;
using QOPIQ.Domain.Models;
using QOPIQ.Domain.Interfaces;
using System.Security.Claims;

namespace QOPIQ.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AuthService> logger,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserDto> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return null;

                var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
                if (!result.Succeeded)
                    return null;

                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = (await _userManager.GetRolesAsync(user))[0] ?? "User"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al autenticar usuario");
                throw;
            }
        }

        public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                
                if (result.Succeeded)
                {
                    // Asignar rol por defecto
                    await _userManager.AddToRoleAsync(user, "User");
                    _logger.LogInformation("Usuario registrado exitosamente: {Email}", registerDto.Email);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                throw;
            }
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(string userId)
        {
            try
            {
                var refreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString("N"),
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 días de validez
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                _unitOfWork.RefreshTokens.Add(refreshToken);
                await _unitOfWork.SaveChangesAsync();

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar token de actualización");
                throw;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return null;

                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = (await _userManager.GetRolesAsync(user))[0] ?? "User"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID");
                throw;
            }
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            try
            {
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
                if (refreshToken == null)
                    return false;

                _unitOfWork.RefreshTokens.Remove(refreshToken);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revocar token de actualización");
                throw;
            }
        }
    }
}
