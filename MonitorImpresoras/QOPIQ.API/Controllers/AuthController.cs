using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOPIQ.Application.DTOs;
using QOPIQ.Application.Interfaces;
using System.Security.Claims;

namespace QOPIQ.API.Controllers
{
    /// <summary>
    /// Controlador de autenticación multi-tenant
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITenantAccessor tenantAccessor,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tenantAccessor = tenantAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Login de usuario en tenant específico
        /// </summary>
        /// <param name="loginDto">Datos de login</param>
        /// <returns>Token JWT y información del usuario</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Tenant ID is required" });
                }

                var result = await _authService.LoginAsync(loginDto, tenantId);
                
                _logger.LogInformation("Successful login for {Email} in tenant {TenantId}", loginDto.Email, tenantId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized login attempt: {Message}", ex.Message);
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Registro de nuevo usuario
        /// </summary>
        /// <param name="registerDto">Datos de registro</param>
        /// <returns>Token JWT y información del usuario</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Tenant ID is required" });
                }

                var result = await _authService.RegisterAsync(registerDto, tenantId);
                
                _logger.LogInformation("Successful registration for {Email} in tenant {TenantId}", registerDto.Email, tenantId);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Renovar token JWT
        /// </summary>
        /// <param name="refreshDto">Token y refresh token</param>
        /// <returns>Nuevo token JWT</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshDto)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Tenant ID is required" });
                }

                var result = await _authService.RefreshTokenAsync(refreshDto, tenantId);
                
                _logger.LogInformation("Token refreshed successfully in tenant {TenantId}", tenantId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
                return Unauthorized(new { message = "Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cerrar sesión
        /// </summary>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tenantId = _tenantAccessor.TenantId;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var result = await _authService.LogoutAsync(userId, tenantId);
                
                if (result)
                {
                    _logger.LogInformation("User {UserId} logged out successfully", userId);
                    return Ok(new { message = "Logged out successfully" });
                }

                return BadRequest(new { message = "Logout failed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cambiar contraseña del usuario actual
        /// </summary>
        /// <param name="changePasswordDto">Datos de cambio de contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tenantId = _tenantAccessor.TenantId;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto, tenantId);
                
                if (result)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                    return Ok(new { message = "Password changed successfully" });
                }

                return BadRequest(new { message = "Password change failed" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Password change failed: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtener información del usuario actual
        /// </summary>
        /// <returns>Información del usuario autenticado</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized(new { message = "No token provided" });
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var userInfo = await _authService.GetUserFromTokenAsync(token);

                if (userInfo == null)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validar token JWT
        /// </summary>
        /// <param name="token">Token a validar</param>
        /// <returns>Resultado de la validación</returns>
        [HttpPost("validate")]
        public async Task<ActionResult> ValidateToken([FromBody] string token)
        {
            try
            {
                var tenantId = _tenantAccessor.TenantId;
                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Tenant ID is required" });
                }

                var isValid = await _authService.ValidateTokenAsync(token, tenantId);
                
                return Ok(new { 
                    isValid,
                    tenantId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Endpoint de prueba para verificar autenticación
        /// </summary>
        /// <returns>Información del contexto de autenticación</returns>
        [HttpGet("test")]
        [Authorize]
        public ActionResult GetAuthTest()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var tenantId = _tenantAccessor.TenantId;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(QopiqClaims.Role)?.Value;

            return Ok(new
            {
                message = "Authentication successful",
                userId,
                tenantId,
                role,
                claims,
                timestamp = DateTime.UtcNow
            });
        }
    }
}

