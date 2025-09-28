using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;

        public AuthController(IAuthService authService, IAuditService auditService)
        {
            _authService = authService;
            _auditService = auditService;
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Inicia sesión de usuario
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            var response = await _authService.LoginAsync(request, ip, userAgent);
            return Ok(response);
        }

        /// <summary>
        /// Renueva token de acceso
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Validar que el refresh token no esté vacío
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                await _auditService.LogAsync("system", "REFRESH_TOKEN_INVALID", "Auth",
                    null, "Refresh token vacío", ip, userAgent);
                return BadRequest("Refresh token es requerido");
            }

            var response = await _authService.RefreshTokenAsync(request, ip);

            if (response == null)
            {
                await _auditService.LogAsync("system", "REFRESH_TOKEN_EXPIRED", "Auth",
                    null, $"Refresh token expirado o inválido: {request.RefreshToken[..Math.Min(10, request.RefreshToken.Length)]}...",
                    ip, userAgent);
                return Unauthorized("Refresh token inválido o expirado");
            }

            // Auditoría de refresh exitoso
            await _auditService.LogAsync(response.UserId, "REFRESH_TOKEN_SUCCESS", "Auth",
                response.UserId, "Token de acceso renovado exitosamente", ip, userAgent);

            return Ok(response);
        }

        /// <summary>
        /// Cierra sesión de usuario
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var result = await _authService.LogoutAsync(userId);
            return Ok(new { success = result });
        }

        /// <summary>
        /// Obtiene información del perfil del usuario autenticado
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProfile()
        {
            var user = User;
            var profile = new
            {
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.FindFirst(ClaimTypes.Name)?.Value,
                Email = user.FindFirst(ClaimTypes.Email)?.Value,
                Roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                IsActive = true // Esto podría venir de la base de datos
            };

            return Ok(profile);
        }
    }
}
