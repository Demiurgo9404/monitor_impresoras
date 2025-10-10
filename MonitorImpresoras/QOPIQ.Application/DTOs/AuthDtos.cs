using System.ComponentModel.DataAnnotations;
using QOPIQ.Domain.Entities;

namespace QOPIQ.Application.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Message { get; set; }
        public UserDto? User { get; set; }
        public DateTime? Expiration { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? CompanyId { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "El token es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "El refresh token es requerido")]
        public string RefreshToken { get; set; } = string.Empty;
        
        public string? IpAddress { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El token de restablecimiento es requerido")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
    }
    
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "El número de teléfono no es válido")]
        public string? PhoneNumber { get; set; }
        
        public string? ProfilePicture { get; set; }
    }
    
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El nombre de la empresa es requerido")]
        public string CompanyName { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        public string? CompanyWebsite { get; set; }
    }
    
    public class ImpersonateRequestDto
    {
        [Required(ErrorMessage = "El ID de usuario es requerido")]
        public string UserId { get; set; } = string.Empty;
    }
}
