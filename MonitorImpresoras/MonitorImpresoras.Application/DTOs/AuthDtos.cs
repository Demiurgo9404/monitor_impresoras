using System.ComponentModel.DataAnnotations;

namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para login multi-tenant
    /// </summary>
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Opcional: si no se proporciona, se toma del header X-Tenant-Id
        /// </summary>
        public string? TenantId { get; set; }
    }

    /// <summary>
    /// DTO para registro de usuario
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? Department { get; set; }

        /// <summary>
        /// Rol inicial del usuario
        /// </summary>
        public string Role { get; set; } = "Viewer";

        /// <summary>
        /// ID de la empresa (opcional, se puede asignar después)
        /// </summary>
        public Guid? CompanyId { get; set; }
    }

    /// <summary>
    /// Respuesta de autenticación exitosa
    /// </summary>
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfoDto User { get; set; } = new();
        public TenantInfoDto Tenant { get; set; } = new();
    }

    /// <summary>
    /// Información del usuario autenticado
    /// </summary>
    public class UserInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string[] Permissions { get; set; } = Array.Empty<string>();
        public Guid? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    /// <summary>
    /// Información del tenant
    /// </summary>
    public class TenantInfoDto
    {
        public string TenantId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Tier { get; set; } = string.Empty;
        public int MaxPrinters { get; set; }
        public int MaxUsers { get; set; }
    }

    /// <summary>
    /// DTO para renovar token
    /// </summary>
    public class RefreshTokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para cambio de contraseña
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Claims personalizados para JWT
    /// </summary>
    public static class QopiqClaims
    {
        public const string TenantId = "tenantId";
        public const string CompanyId = "companyId";
        public const string Role = "role";
        public const string Permissions = "permissions";
        public const string FullName = "fullName";
    }

    /// <summary>
    /// Roles del sistema
    /// </summary>
    public static class QopiqRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string CompanyAdmin = "CompanyAdmin";
        public const string ProjectManager = "ProjectManager";
        public const string Technician = "Technician";
        public const string Viewer = "Viewer";

        public static readonly string[] All = {
            SuperAdmin, CompanyAdmin, ProjectManager, Technician, Viewer
        };

        public static readonly string[] AdminRoles = {
            SuperAdmin, CompanyAdmin
        };

        public static readonly string[] ManagerRoles = {
            SuperAdmin, CompanyAdmin, ProjectManager
        };
    }

    /// <summary>
    /// Permisos del sistema
    /// </summary>
    public static class QopiqPermissions
    {
        // Impresoras
        public const string ReadPrinters = "read:printers";
        public const string WritePrinters = "write:printers";
        public const string DeletePrinters = "delete:printers";

        // Proyectos
        public const string ReadProjects = "read:projects";
        public const string WriteProjects = "write:projects";
        public const string DeleteProjects = "delete:projects";

        // Empresas
        public const string ReadCompanies = "read:companies";
        public const string WriteCompanies = "write:companies";
        public const string DeleteCompanies = "delete:companies";

        // Usuarios
        public const string ReadUsers = "read:users";
        public const string WriteUsers = "write:users";
        public const string DeleteUsers = "delete:users";

        // Reportes
        public const string ReadReports = "read:reports";
        public const string WriteReports = "write:reports";
        public const string DeleteReports = "delete:reports";

        // Administración
        public const string ManageTenant = "manage:tenant";
        public const string ViewLogs = "view:logs";
        public const string ManageSettings = "manage:settings";

        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            [QopiqRoles.SuperAdmin] = {
                ReadPrinters, WritePrinters, DeletePrinters,
                ReadProjects, WriteProjects, DeleteProjects,
                ReadCompanies, WriteCompanies, DeleteCompanies,
                ReadUsers, WriteUsers, DeleteUsers,
                ReadReports, WriteReports, DeleteReports,
                ManageTenant, ViewLogs, ManageSettings
            },
            [QopiqRoles.CompanyAdmin] = {
                ReadPrinters, WritePrinters, DeletePrinters,
                ReadProjects, WriteProjects, DeleteProjects,
                ReadUsers, WriteUsers,
                ReadReports, WriteReports, DeleteReports,
                ViewLogs, ManageSettings
            },
            [QopiqRoles.ProjectManager] = {
                ReadPrinters, WritePrinters,
                ReadProjects, WriteProjects,
                ReadUsers,
                ReadReports, WriteReports
            },
            [QopiqRoles.Technician] = {
                ReadPrinters, WritePrinters,
                ReadProjects,
                ReadReports
            },
            [QopiqRoles.Viewer] = {
                ReadPrinters,
                ReadProjects,
                ReadReports
            }
        };
    }
}
