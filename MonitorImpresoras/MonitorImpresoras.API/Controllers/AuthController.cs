using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Constants;
using Serilog;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IAuditService auditService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        /// <param name="request">Datos del nuevo usuario</param>
        /// <returns>Resultado del registro</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            try
            {
                _logger.LogInformation("Intento de registro de usuario: {Email}", request.Email);

                var result = await _authService.RegisterAsync(request);

                if (result)
                {
                    _logger.LogInformation("Usuario registrado exitosamente: {Email}", request.Email);
                    return Ok(new { success = true, message = "Usuario registrado exitosamente" });
                }
                else
                {
                    _logger.LogWarning("Error en registro de usuario: {Email}", request.Email);
                    return BadRequest(new { success = false, message = "Error al registrar usuario" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al registrar usuario: {Email}", request.Email);
                throw; // El middleware global manejará el error
            }
        }

        /// <summary>
        /// Inicia sesión de usuario
        /// </summary>
        /// <param name="request">Credenciales de usuario</param>
        /// <returns>Token de acceso y refresh token</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("Intento de login desde IP {Ip} con User-Agent: {UserAgent}", ip, userAgent);

                var response = await _authService.LoginAsync(request, ip, userAgent);

                if (response != null)
                {
                    _logger.LogInformation("Login exitoso para usuario: {Username} desde IP: {Ip}", request.Username, ip);

                    // Auditoría de login exitoso
                    await _auditService.LogAsync(response.UserId, "LOGIN_SUCCESS", "User", response.UserId,
                        $"Login exitoso desde IP {ip}", ip, userAgent);

                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Login fallido para usuario: {Username} desde IP: {Ip}", request.Username, ip);

                    // Auditoría de login fallido
                    await _auditService.LogAsync("system", "LOGIN_FAILED", "User", null,
                        $"Login fallido para {request.Username} desde IP {ip}", ip, userAgent);

                    return Unauthorized(new { errorCode = ErrorCodes.InvalidCredentials, message = "Credenciales inválidas" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en login para usuario: {Username}", request.Username);
                throw;
            }
        }

        /// <summary>
        /// Renueva token de acceso
        /// </summary>
        /// <param name="request">Refresh token válido</param>
        /// <returns>Nuevo token de acceso</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("Intento de refresh token desde IP: {Ip}", ip);

                // Validar que el refresh token no esté vacío
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    _logger.LogWarning("Refresh token vacío desde IP: {Ip}", ip);
                    await _auditService.LogAsync("system", "REFRESH_TOKEN_INVALID", "Auth",
                        null, "Refresh token vacío", ip, userAgent);
                    return BadRequest(new { errorCode = ErrorCodes.InvalidInputFormat, message = "Refresh token es requerido" });
                }

                var response = await _authService.RefreshTokenAsync(request.RefreshToken, ip);

                if (response == null)
                {
                    _logger.LogWarning("Refresh token inválido o expirado desde IP: {Ip}", ip);
                    await _auditService.LogAsync("system", "REFRESH_TOKEN_EXPIRED", "Auth",
                        null, $"Refresh token expirado o inválido desde IP {ip}", ip, userAgent);
                    return Unauthorized(new { errorCode = ErrorCodes.RefreshTokenInvalid, message = "Refresh token inválido o expirado" });
                }

                _logger.LogInformation("Refresh token exitoso para usuario: {UserId} desde IP: {Ip}", response.UserId, ip);

                // Auditoría de refresh exitoso
                await _auditService.LogAsync(response.UserId, "REFRESH_TOKEN_SUCCESS", "Auth",
                    response.UserId, "Token de acceso renovado exitosamente", ip, userAgent);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en refresh token desde IP: {Ip}", HttpContext.Connection.RemoteIpAddress?.ToString());
                throw;
            }
        }

        /// <summary>
        /// Cierra sesión de usuario
        /// </summary>
        /// <returns>Resultado del logout</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                _logger.LogInformation("Logout solicitado para usuario: {UserId} desde IP: {Ip}", userId, ip);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Intento de logout sin usuario identificado desde IP: {Ip}", ip);
                    return Unauthorized(new { errorCode = ErrorCodes.AuthenticationFailed, message = "Usuario no autenticado" });
                }

                var result = await _authService.LogoutAsync(userId);

                if (result)
                {
                    _logger.LogInformation("Logout exitoso para usuario: {UserId}", userId);
                    return Ok(new { success = true, message = "Sesión cerrada exitosamente" });
                }
                else
                {
                    _logger.LogWarning("Error en logout para usuario: {UserId}", userId);
                    return BadRequest(new { success = false, message = "Error al cerrar sesión" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en logout para usuario: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                throw;
            }
        }

        /// <summary>
        /// Obtiene información del perfil del usuario autenticado
        /// </summary>
        /// <returns>Información del usuario</returns>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                _logger.LogInformation("Perfil consultado para usuario: {Username} ({UserId})", username, userId);

                var profile = new
                {
                    UserId = userId,
                    Username = username,
                    Email = email,
                    Roles = roles,
                    IsActive = User.HasClaim("IsActive", "true"),
                    Department = User.FindFirst("Department")?.Value,
                    Permissions = User.FindFirst("Permissions")?.Value
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil para usuario: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                throw;
            }
        }
    }
}
