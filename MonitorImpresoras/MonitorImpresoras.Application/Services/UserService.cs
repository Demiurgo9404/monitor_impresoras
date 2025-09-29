using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using System.Linq.Expressions;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para gestión avanzada de usuarios con auditoría y seguridad
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IAuditService auditService,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(
            string? searchTerm = null,
            bool? isActive = null,
            string? role = null,
            int page = 1,
            int pageSize = 10)
        {
            _logger.LogInformation("Obteniendo lista de usuarios paginada: página {Page}, tamaño {PageSize}", page, pageSize);

            var query = _userManager.Users.AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.UserName!.Contains(searchTerm) ||
                    u.Email!.Contains(searchTerm) ||
                    u.FirstName!.Contains(searchTerm) ||
                    u.LastName!.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIdsInRole = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            // Obtener total antes de paginación
            var totalCount = await query.CountAsync();

            // Aplicar paginación
            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mapear a DTOs
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!,
                    Department = user.Department,
                    IsActive = user.IsActive,
                    LastLoginAtUtc = user.LastLoginAtUtc,
                    FailedLoginAttempts = user.FailedLoginAttempts,
                    LockedUntilUtc = user.LockedUntilUtc,
                    CreatedAtUtc = user.CreatedAtUtc,
                    Roles = roles.ToList()
                });
            }

            _logger.LogInformation("Se encontraron {Count} usuarios (total: {TotalCount})", userDtos.Count, totalCount);

            return new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(string userId)
        {
            _logger.LogInformation("Obteniendo detalles del usuario: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Where(r => r.IsActive).ToListAsync();

            var userDetail = new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Department = user.Department,
                IsActive = user.IsActive,
                LastLoginAtUtc = user.LastLoginAtUtc,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockedUntilUtc = user.LockedUntilUtc,
                CreatedAtUtc = user.CreatedAtUtc,
                UpdatedAtUtc = user.UpdatedAtUtc,
                CreatedByUserId = user.CreatedByUserId,
                UpdatedByUserId = user.UpdatedByUserId,
                CurrentRoles = roles.ToList(),
                AvailableRoles = allRoles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    Description = r.Description,
                    PermissionLevel = r.PermissionLevel,
                    IsSystemRole = r.IsSystemRole
                }).ToList()
            };

            _logger.LogInformation("Detalles obtenidos para usuario: {UserName}", user.UserName);
            return userDetail;
        }

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleName, string performedByUserId)
        {
            _logger.LogInformation("Asignando rol {RoleName} al usuario {UserId} por {PerformedByUserId}",
                roleName, userId, performedByUserId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para asignar rol: {UserId}", userId);
                return false;
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null || !role.IsActive)
            {
                _logger.LogWarning("Rol no encontrado o inactivo: {RoleName}", roleName);
                return false;
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Error al asignar rol {RoleName} al usuario {UserId}: {Errors}",
                    roleName, userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                return false;
            }

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "ROLE_ASSIGNED", "UserRole",
                $"{userId}:{roleName}", $"Rol {roleName} asignado a usuario {user.UserName}");

            _logger.LogInformation("Rol {RoleName} asignado exitosamente al usuario {UserName}", roleName, user.UserName);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleName, string performedByUserId)
        {
            _logger.LogInformation("Removiendo rol {RoleName} del usuario {UserId} por {PerformedByUserId}",
                roleName, userId, performedByUserId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para remover rol: {UserId}", userId);
                return false;
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Error al remover rol {RoleName} del usuario {UserId}: {Errors}",
                    roleName, userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                return false;
            }

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "ROLE_REMOVED", "UserRole",
                $"{userId}:{roleName}", $"Rol {roleName} removido de usuario {user.UserName}");

            _logger.LogInformation("Rol {RoleName} removido exitosamente del usuario {UserName}", roleName, user.UserName);
            return true;
        }

        public async Task<bool> BlockUserAsync(string userId, string performedByUserId, string reason = "Bloqueado por administrador")
        {
            _logger.LogInformation("Bloqueando usuario {UserId} por {PerformedByUserId}. Razón: {Reason}",
                userId, performedByUserId, reason);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para bloquear: {UserId}", userId);
                return false;
            }

            user.IsActive = false;
            user.LockedUntilUtc = DateTime.UtcNow.AddDays(365); // Bloqueo permanente hasta desbloqueo manual

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Error al bloquear usuario {UserId}: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                return false;
            }

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "USER_BLOCKED", "User", userId,
                $"Usuario {user.UserName} bloqueado. Razón: {reason}");

            _logger.LogInformation("Usuario {UserName} bloqueado exitosamente", user.UserName);
            return true;
        }

        public async Task<bool> UnblockUserAsync(string userId, string performedByUserId)
        {
            _logger.LogInformation("Desbloqueando usuario {UserId} por {PerformedByUserId}", userId, performedByUserId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para desbloquear: {UserId}", userId);
                return false;
            }

            user.IsActive = true;
            user.LockedUntilUtc = null;
            user.FailedLoginAttempts = 0;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Error al desbloquear usuario {UserId}: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                return false;
            }

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "USER_UNBLOCKED", "User", userId,
                $"Usuario {user.UserName} desbloqueado");

            _logger.LogInformation("Usuario {UserName} desbloqueado exitosamente", user.UserName);
            return true;
        }

        public async Task<bool> ResetUserPasswordAsync(string userId, string newPassword, string performedByUserId)
        {
            _logger.LogInformation("Reseteando contraseña del usuario {UserId} por {PerformedByUserId}", userId, performedByUserId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para resetear contraseña: {UserId}", userId);
                return false;
            }

            // Remover contraseña actual
            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                _logger.LogError("Error al remover contraseña actual del usuario {UserId}: {Errors}",
                    userId, string.Join("; ", removePasswordResult.Errors.Select(e => e.Description)));
                return false;
            }

            // Establecer nueva contraseña
            var addPasswordResult = await _userManager.AddPasswordAsync(user, newPassword);
            if (!addPasswordResult.Succeeded)
            {
                _logger.LogError("Error al establecer nueva contraseña para usuario {UserId}: {Errors}",
                    userId, string.Join("; ", addPasswordResult.Errors.Select(e => e.Description)));
                return false;
            }

            // Resetear intentos fallidos
            user.FailedLoginAttempts = 0;
            user.LockedUntilUtc = null;
            await _userManager.UpdateAsync(user);

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "PASSWORD_RESET", "User", userId,
                $"Contraseña reseteada para usuario {user.UserName}");

            _logger.LogInformation("Contraseña reseteada exitosamente para usuario {UserName}", user.UserName);
            return true;
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto profileDto, string performedByUserId)
        {
            _logger.LogInformation("Actualizando perfil del usuario {UserId} por {PerformedByUserId}", userId, performedByUserId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para actualizar perfil: {UserId}", userId);
                return false;
            }

            // Verificar si es el propio usuario o un admin
            var isSelfUpdate = userId == performedByUserId;
            var performedByUser = await _userManager.FindByIdAsync(performedByUserId);
            var performedByRoles = performedByUser != null ? await _userManager.GetRolesAsync(performedByUser) : new List<string>();

            // Solo admins pueden cambiar ciertos campos
            if (!isSelfUpdate && !performedByRoles.Contains("Admin"))
            {
                _logger.LogWarning("Usuario {PerformedByUserId} no tiene permisos para actualizar perfil de {UserId}",
                    performedByUserId, userId);
                return false;
            }

            // Actualizar campos permitidos
            var changes = new List<string>();

            if (!string.IsNullOrEmpty(profileDto.FirstName) && profileDto.FirstName != user.FirstName)
            {
                user.FirstName = profileDto.FirstName;
                changes.Add($"FirstName: {user.FirstName} -> {profileDto.FirstName}");
            }

            if (!string.IsNullOrEmpty(profileDto.LastName) && profileDto.LastName != user.LastName)
            {
                user.LastName = profileDto.LastName;
                changes.Add($"LastName: {user.LastName} -> {profileDto.LastName}");
            }

            if (!string.IsNullOrEmpty(profileDto.Department) && profileDto.Department != user.Department)
            {
                user.Department = profileDto.Department;
                changes.Add($"Department: {user.Department} -> {profileDto.Department}");
            }

            // Solo admins pueden cambiar el estado activo
            if (performedByRoles.Contains("Admin") && profileDto.IsActive.HasValue && profileDto.IsActive.Value != user.IsActive)
            {
                user.IsActive = profileDto.IsActive.Value;
                changes.Add($"IsActive: {user.IsActive} -> {profileDto.IsActive.Value}");
            }

            if (changes.Any())
            {
                user.UpdatedAtUtc = DateTime.UtcNow;
                user.UpdatedByUserId = performedByUserId;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Error al actualizar perfil del usuario {UserId}: {Errors}",
                        userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                    return false;
                }

                // Auditoría
                await _auditService.LogAsync(performedByUserId, "USER_PROFILE_UPDATED", "User", userId,
                    $"Perfil actualizado: {string.Join(", ", changes)}");

                _logger.LogInformation("Perfil actualizado exitosamente para usuario {UserName}: {Changes}",
                    user.UserName, string.Join(", ", changes));
            }

            return true;
        }
    }
}
