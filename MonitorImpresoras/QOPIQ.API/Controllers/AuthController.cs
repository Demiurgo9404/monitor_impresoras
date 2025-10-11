using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QOPIQ.Application.Interfaces;
using QOPIQ.Domain.Models;

namespace QOPIQ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);
                if (user == null)
                    return Unauthorized(new { message = "Usuario o contraseña incorrectos" });

                var token = GenerateJwtToken(user);
                var refreshToken = await _authService.GenerateRefreshTokenAsync(user.Id);

                return Ok(new
                {
                    Token = token,
                    RefreshToken = refreshToken.Token,
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60") * 60
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el inicio de sesión");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (!result.Succeeded)
                    return BadRequest(new { message = "Error al registrar el usuario", errors = result.Errors });

                return Ok(new { message = "Usuario registrado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { message = "Token inválido" });

                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null || user.RefreshToken != refreshTokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                    return BadRequest(new { message = "Token de refresco inválido o expirado" });

                var newJwtToken = GenerateJwtToken(user);
                var newRefreshToken = await _authService.GenerateRefreshTokenAsync(userId);

                return Ok(new
                {
                    Token = newJwtToken,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresIn = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60") * 60
                });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Error al refrescar el token");
                return BadRequest(new { message = "Token inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar el token");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private string GenerateJwtToken(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false, // No validamos la expiración aquí
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token inválido");

            return principal;
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
