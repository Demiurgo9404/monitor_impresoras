using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;

namespace MonitorImpresoras.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                var response = await _authService.LoginAsync(loginRequest);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Invalid login attempt for {Email}", loginRequest.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginRequest.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerRequest);
                _logger.LogInformation("User {Email} registered successfully", registerRequest.Email);
                return CreatedAtAction(nameof(Login), new { email = registerRequest.Email }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Registration failed for {Email}", registerRequest.Email);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", registerRequest.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
                return Ok(response);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid refresh token attempt");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { message = "An error occurred while refreshing token" });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var result = await _authService.RevokeTokenAsync(userId);
            if (!result)
            {
                return BadRequest(new { message = "Failed to revoke token" });
            }

            return Ok(new { message = "Token revoked successfully" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid user" });
            }

            var result = await _authService.ChangePasswordAsync(
                userId, 
                changePasswordDto.CurrentPassword, 
                changePasswordDto.NewPassword);

            if (!result)
            {
                return BadRequest(new { message = "Failed to change password" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
    }

    public class RefreshTokenRequestDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
