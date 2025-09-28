using FluentValidation;
using MonitorImpresoras.Application.DTOs.Printers;

namespace MonitorImpresoras.Application.Validators.Printers
{
    /// <summary>
    /// Validador para CreatePrinterDto usando FluentValidation
    /// </summary>
    public class CreatePrinterDtoValidator : AbstractValidator<CreatePrinterDto>
    {
        public CreatePrinterDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres");

            RuleFor(p => p.Model)
                .NotEmpty().WithMessage("El modelo es requerido")
                .MaximumLength(100).WithMessage("El modelo no puede exceder los 100 caracteres");

            RuleFor(p => p.SerialNumber)
                .NotEmpty().WithMessage("El número de serie es requerido")
                .MaximumLength(100).WithMessage("El número de serie no puede exceder los 100 caracteres");

            RuleFor(p => p.IpAddress)
                .NotEmpty().WithMessage("La dirección IP es requerida")
                .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")
                .WithMessage("Debe ser una dirección IP válida (ej: 192.168.1.1)")
                .MaximumLength(50).WithMessage("La dirección IP no puede exceder los 50 caracteres");

            RuleFor(p => p.Location)
                .MaximumLength(200).WithMessage("La ubicación no puede exceder los 200 caracteres");

            RuleFor(p => p.Status)
                .MaximumLength(50).WithMessage("El estado no puede exceder los 50 caracteres");

            RuleFor(p => p.CommunityString)
                .MaximumLength(50).WithMessage("La cadena de comunidad no puede exceder los 50 caracteres");

            RuleFor(p => p.SnmpPort)
                .InclusiveBetween(1, 65535).When(p => p.SnmpPort.HasValue)
                .WithMessage("El puerto SNMP debe estar entre 1 y 65535");

            RuleFor(p => p.Notes)
                .MaximumLength(500).WithMessage("Las notas no pueden exceder los 500 caracteres");
        }
    }
}
