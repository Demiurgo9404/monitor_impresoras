namespace MonitorImpresoras.Application.DTOs
{
    /// <summary>
    /// DTO para representar usuarios en listados
    /// </summary>
    public class UserDto
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? Department { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// DTO para detalles completos de usuario
    /// </summary>
    public class UserDetailDto : UserDto
    {
        public DateTime? UpdatedAtUtc { get; set; }
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
        public List<string> CurrentRoles { get; set; } = new();
        public List<RoleDto> AvailableRoles { get; set; } = new();
    }

    /// <summary>
    /// DTO para representar roles
    /// </summary>
    public class RoleDto
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int PermissionLevel { get; set; }
        public bool IsSystemRole { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int UserCount { get; set; }
    }

    /// <summary>
    /// DTO para actualizar perfil de usuario
    /// </summary>
    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO para filtros de búsqueda de usuarios
    /// </summary>
    public class UserSearchDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string? Role { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// DTO para resultado paginado
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
