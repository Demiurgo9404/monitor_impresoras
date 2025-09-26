using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);
            
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var token = _jwtService.GenerateJwtToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            
            // Save refresh token to user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpirationInDays"]));
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerRequest.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                UserName = registerRequest.Email,
                Email = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Department = registerRequest.Department,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Assign default role if specified, otherwise assign User role
            var roleToAssign = !string.IsNullOrEmpty(registerRequest.Role) ? registerRequest.Role : "User";
            var roleExists = await _userManager.IsInRoleAsync(user, roleToAssign);

            if (!roleExists)
            {
                await _userManager.AddToRoleAsync(user, roleToAssign);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateJwtToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token to user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:RefreshTokenExpirationInDays"]));
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newToken = _jwtService.GenerateJwtToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            
            // Update refresh token
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = roles.ToList()
            };
        }

        public async Task<bool> RevokeTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            var result = await _userManager.UpdateAsync(user);
            
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }
    }
}
