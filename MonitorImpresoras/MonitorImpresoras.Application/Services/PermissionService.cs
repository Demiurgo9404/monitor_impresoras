using Microsoft.EntityFrameworkCore;
using MonitorImpresoras.Application.DTOs;
using MonitorImpresoras.Application.Interfaces;
using MonitorImpresoras.Domain.Entities;
using MonitorImpresoras.Infrastructure.Data;

namespace MonitorImpresoras.Application.Services
{
    /// <summary>
    /// Servicio para gestión de permisos granulares (claims) de usuarios
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            ApplicationDbContext context,
            IAuditService auditService,
            ILogger<PermissionService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<IEnumerable<UserClaimDto>> GetUserClaimsAsync(string userId)
        {
            _logger.LogInformation("Obteniendo claims del usuario: {UserId}", userId);

            var claims = await _context.UserClaims
                .Where(uc => uc.UserId == userId && uc.IsValid)
                .OrderBy(uc => uc.Category)
                .ThenBy(uc => uc.ClaimType)
                .Select(uc => new UserClaimDto
                {
                    Id = uc.Id,
                    ClaimType = uc.ClaimType,
                    ClaimValue = uc.ClaimValue,
                    Description = uc.Description,
                    Category = uc.Category,
                    IsActive = uc.IsActive,
                    CreatedAtUtc = uc.CreatedAtUtc,
                    ExpiresAtUtc = uc.ExpiresAtUtc,
                    CreatedByUserId = uc.CreatedByUserId
                })
                .ToListAsync();

            _logger.LogInformation("Se encontraron {Count} claims válidos para el usuario {UserId}", claims.Count, userId);
            return claims;
        }

        public async Task<bool> AssignClaimToUserAsync(
            string userId,
            string claimType,
            string claimValue,
            string? description = null,
            string? category = null,
            DateTime? expiresAtUtc = null,
            string performedByUserId = "system")
        {
            _logger.LogInformation("Asignando claim {ClaimType}={ClaimValue} al usuario {UserId} por {PerformedByUserId}",
                claimType, claimValue, userId, performedByUserId);

            // Verificar si el usuario existe
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para asignar claim: {UserId}", userId);
                return false;
            }

            // Verificar si ya tiene este claim
            var existingClaim = await _context.UserClaims
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimType == claimType && uc.IsActive);

            if (existingClaim != null)
            {
                // Si ya tiene el mismo claim, solo actualizar si es diferente
                if (existingClaim.ClaimValue != claimValue || existingClaim.Description != description)
                {
                    existingClaim.ClaimValue = claimValue;
                    existingClaim.Description = description;
                    existingClaim.Category = category;
                    existingClaim.ExpiresAtUtc = expiresAtUtc;
                    existingClaim.UpdatedAtUtc = DateTime.UtcNow;
                    existingClaim.UpdatedByUserId = performedByUserId;
                }
            }
            else
            {
                // Crear nuevo claim
                var newClaim = new UserClaim
                {
                    UserId = userId,
                    ClaimType = claimType,
                    ClaimValue = claimValue,
                    Description = description,
                    Category = category,
                    ExpiresAtUtc = expiresAtUtc,
                    CreatedByUserId = performedByUserId,
                    IsActive = true
                };

                _context.UserClaims.Add(newClaim);
            }

            await _context.SaveChangesAsync();

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "CLAIM_ASSIGNED", "UserClaim",
                $"{userId}:{claimType}", $"Claim {claimType}={claimValue} asignado a usuario {user.UserName}");

            _logger.LogInformation("Claim {ClaimType}={ClaimValue} asignado exitosamente al usuario {UserName}",
                claimType, claimValue, user.UserName);
            return true;
        }

        public async Task<bool> RevokeClaimFromUserAsync(
            string userId,
            string claimType,
            string performedByUserId = "system")
        {
            _logger.LogInformation("Revocando claim {ClaimType} del usuario {UserId} por {PerformedByUserId}",
                claimType, userId, performedByUserId);

            var claim = await _context.UserClaims
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimType == claimType && uc.IsActive);

            if (claim == null)
            {
                _logger.LogWarning("Claim no encontrado para revocar: {ClaimType} del usuario {UserId}", claimType, userId);
                return false;
            }

            claim.IsActive = false;
            claim.UpdatedAtUtc = DateTime.UtcNow;
            claim.UpdatedByUserId = performedByUserId;

            await _context.SaveChangesAsync();

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "CLAIM_REVOKED", "UserClaim",
                $"{userId}:{claimType}", $"Claim {claimType} revocado de usuario");

            _logger.LogInformation("Claim {ClaimType} revocado exitosamente del usuario {UserId}", claimType, userId);
            return true;
        }

        public async Task<bool> RevokeAllClaimsFromUserAsync(
            string userId,
            string performedByUserId = "system",
            string reason = "Revocación masiva de claims")
        {
            _logger.LogInformation("Revocando todos los claims del usuario {UserId} por {PerformedByUserId}. Razón: {Reason}",
                userId, performedByUserId, reason);

            var claims = await _context.UserClaims
                .Where(uc => uc.UserId == userId && uc.IsActive)
                .ToListAsync();

            if (!claims.Any())
            {
                _logger.LogInformation("No hay claims activos para revocar del usuario {UserId}", userId);
                return true;
            }

            foreach (var claim in claims)
            {
                claim.IsActive = false;
                claim.UpdatedAtUtc = DateTime.UtcNow;
                claim.UpdatedByUserId = performedByUserId;
            }

            await _context.SaveChangesAsync();

            // Auditoría
            await _auditService.LogAsync(performedByUserId, "CLAIMS_REVOKED_ALL", "UserClaim",
                userId, $"Todos los claims revocados de usuario. Razón: {reason}");

            _logger.LogInformation("Se revocaron {Count} claims del usuario {UserId}", claims.Count, userId);
            return true;
        }

        public async Task<IEnumerable<ClaimDefinitionDto>> GetAvailableClaimsAsync()
        {
            _logger.LogDebug("Obteniendo lista de claims disponibles");

            // Claims predefinidos del sistema
            var systemClaims = new List<ClaimDefinitionDto>
            {
                new ClaimDefinitionDto
                {
                    ClaimType = "printers.manage",
                    ClaimValue = "true",
                    Description = "Permite gestionar impresoras (crear, editar, eliminar)",
                    Category = "printers"
                },
                new ClaimDefinitionDto
                {
                    ClaimType = "printers.view",
                    ClaimValue = "true",
                    Description = "Permite ver información de impresoras",
                    Category = "printers"
                },
                new ClaimDefinitionDto
                {
                    ClaimType = "reports.view",
                    ClaimValue = "true",
                    Description = "Permite ver reportes del sistema",
                    Category = "reports"
                },
                new ClaimDefinitionDto
                {
                    ClaimType = "users.manage",
                    ClaimValue = "true",
                    Description = "Permite gestionar usuarios (bloquear, roles, etc.)",
                    Category = "users"
                },
                new ClaimDefinitionDto
                {
                    ClaimType = "audit.view",
                    ClaimValue = "true",
                    Description = "Permite ver logs de auditoría",
                    Category = "system"
                },
                new ClaimDefinitionDto
                {
                    ClaimType = "system.admin",
                    ClaimValue = "true",
                    Description = "Permisos administrativos completos",
                    Category = "system"
                }
            };

            return systemClaims;
        }

        public async Task<bool> UserHasClaimAsync(string userId, string claimType, string claimValue = "true")
        {
            var hasClaim = await _context.UserClaims
                .AnyAsync(uc => uc.UserId == userId &&
                               uc.ClaimType == claimType &&
                               uc.ClaimValue == claimValue &&
                               uc.IsValid);

            _logger.LogDebug("Usuario {UserId} tiene claim {ClaimType}={ClaimValue}: {HasClaim}",
                userId, claimType, claimValue, hasClaim);

            return hasClaim;
        }

        public async Task<IEnumerable<string>> GetUserClaimTypesAsync(string userId)
        {
            var claimTypes = await _context.UserClaims
                .Where(uc => uc.UserId == userId && uc.IsValid)
                .Select(uc => uc.ClaimType)
                .Distinct()
                .ToListAsync();

            _logger.LogDebug("Usuario {UserId} tiene {Count} tipos de claims", userId, claimTypes.Count);
            return claimTypes;
        }

        public async Task<Dictionary<string, string>> GetUserClaimsDictionaryAsync(string userId)
        {
            var claims = await _context.UserClaims
                .Where(uc => uc.UserId == userId && uc.IsValid)
                .ToDictionaryAsync(uc => uc.ClaimType, uc => uc.ClaimValue);

            _logger.LogDebug("Usuario {UserId} tiene {Count} claims activos", userId, claims.Count);
            return claims;
        }
    }
}
