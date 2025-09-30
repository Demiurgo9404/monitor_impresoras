using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Constants;
using System.Security.Claims;

namespace MonitorImpresoras.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene lista paginada de usuarios (solo admin)
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <param name="isActive">Filtrar por estado activo</param>
        /// <param name="role">Filtrar por rol</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de usuarios</returns>
        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? role = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Obteniendo usuarios - SearchTerm: {SearchTerm}, Page: {Page}, PageSize: {PageSize}", searchTerm, page, pageSize);

                var result = await _userService.GetUsersAsync(searchTerm, isActive, role, page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de usuarios");
                throw;
            }
        }

        /// <summary>
        /// Obtiene detalles completos de un usuario específico
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Detalles del usuario</returns>
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                _logger.LogInformation("Obteniendo detalles del usuario: {UserId}", id);

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { errorCode = ErrorCodes.EntityNotFound, message = "Usuario no encontrado" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del usuario: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Asigna un rol a un usuario (solo admin)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="request">Datos del rol a asignar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/roles")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleDto request)
        {
            try
            {
                var performedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Asignando rol {RoleName} al usuario {UserId} por {PerformedByUserId}",
                    request.RoleName, id, performedByUserId);

                var result = await _userService.AssignRoleToUserAsync(id, request.RoleName, performedByUserId);

                if (!result)
                {
                    return BadRequest(new { errorCode = ErrorCodes.ValidationFailed, message = "No se pudo asignar el rol" });
                }

                return Ok(new { success = true, message = "Rol asignado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar rol al usuario: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Remueve un rol de un usuario (solo admin)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="role">Nombre del rol a remover</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}/roles/{role}")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            try
            {
                var performedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Removiendo rol {RoleName} del usuario {UserId} por {PerformedByUserId}",
                    role, id, performedByUserId);

                var result = await _userService.RemoveRoleFromUserAsync(id, role, performedByUserId);

                if (!result)
                {
                    return BadRequest(new { errorCode = ErrorCodes.ValidationFailed, message = "No se pudo remover el rol" });
                }

                return Ok(new { success = true, message = "Rol removido exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al remover rol del usuario: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Bloquea un usuario (solo admin)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="request">Datos del bloqueo</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/block")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BlockUser(string id, [FromBody] BlockUserDto request)
        {
            try
            {
                var performedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Bloqueando usuario {UserId} por {PerformedByUserId}. Razón: {Reason}",
                    id, performedByUserId, request.Reason);

                var result = await _userService.BlockUserAsync(id, performedByUserId, request.Reason);

                if (!result)
                {
                    return BadRequest(new { errorCode = ErrorCodes.ValidationFailed, message = "No se pudo bloquear el usuario" });
                }

                return Ok(new { success = true, message = "Usuario bloqueado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al bloquear usuario: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Desbloquea un usuario (solo admin)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/unblock")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnblockUser(string id)
        {
            try
            {
                var performedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Desbloqueando usuario {UserId} por {PerformedByUserId}", id, performedByUserId);

                var result = await _userService.UnblockUserAsync(id, performedByUserId);

                if (!result)
                {
                    return BadRequest(new { errorCode = ErrorCodes.ValidationFailed, message = "No se pudo desbloquear el usuario" });
                }

                return Ok(new { success = true, message = "Usuario desbloqueado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desbloquear usuario: {UserId}", id);
                throw;
            }
        }

        /// <summary>
        /// Resetea la contraseña de un usuario (solo admin)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="request">Nueva contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPut("{id}/reset-password")]
        [Authorize(Policy = "RequireAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto request)
        {
            try
            {
                var performedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

                _logger.LogInformation("Reseteando contraseña del usuario {UserId} por {PerformedByUserId}", id, performedByUserId);

                var result = await _userService.ResetUserPasswordAsync(id, request.NewPassword, performedByUserId);

                if (!result)
                {
                    return BadRequest(new { errorCode = ErrorCodes.ValidationFailed, message = "No se pudo resetear la contraseña" });
                }

                return Ok(new { success = true, message = "Contraseña reseteada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña del usuario: {UserId}", id);
                throw;
            }
        }
        // Close UsersController class before DTOs
    }

    // DTOs auxiliares para las peticiones
    public class AssignRoleDto
    {
        public string RoleName { get; set; } = default!;
    }

    public class BlockUserDto
    {
        public string Reason { get; set; } = "Bloqueado por administrador";
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = default!;
    }
}
