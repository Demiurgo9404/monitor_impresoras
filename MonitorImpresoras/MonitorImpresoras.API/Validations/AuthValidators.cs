using FluentValidation;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.API.Validations
{
    public class LoginRequestDTOValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no tiene un formato válido")
                .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");
        }
    }

    public class RegisterRequestDTOValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El email no tiene un formato válido")
                .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número")
                .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido")
                .MaximumLength(50).WithMessage("El apellido no puede exceder 50 caracteres");

            RuleFor(x => x.Department)
                .MaximumLength(100).WithMessage("El departamento no puede exceder 100 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Department));

            RuleFor(x => x.Role)
                .Must(role => string.IsNullOrEmpty(role) || new[] { "Admin", "Technician", "User" }.Contains(role))
                .WithMessage("El rol debe ser Admin, Technician o User");
        }
    }
}
