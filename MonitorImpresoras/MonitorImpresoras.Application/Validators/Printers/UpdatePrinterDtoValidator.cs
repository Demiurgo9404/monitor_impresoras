using FluentValidation;
using MonitorImpresoras.Application.DTOs.Printers;

namespace MonitorImpresoras.Application.Validators.Printers
{
    /// <summary>
    /// Validador para UpdatePrinterDto usando FluentValidation
    /// </summary>
    public class UpdatePrinterDtoValidator : AbstractValidator<UpdatePrinterDto>
    {
        public UpdatePrinterDtoValidator()
        {
            RuleFor(p => p.Name)
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres")
                .When(p => p.Name != null);

            RuleFor(p => p.Model)
                .MaximumLength(100).WithMessage("El modelo no puede exceder los 100 caracteres")
                .When(p => p.Model != null);

            RuleFor(p => p.SerialNumber)
                .MaximumLength(100).WithMessage("El número de serie no puede exceder los 100 caracteres")
                .When(p => p.SerialNumber != null);

            RuleFor(p => p.IpAddress)
                .Matches(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")
                .WithMessage("Debe ser una dirección IP válida (ej: 192.168.1.1)")
                .MaximumLength(50).WithMessage("La dirección IP no puede exceder los 50 caracteres")
                .When(p => p.IpAddress != null);

            RuleFor(p => p.Location)
                .MaximumLength(200).WithMessage("La ubicación no puede exceder los 200 caracteres")
                .When(p => p.Location != null);

            RuleFor(p => p.Status)
                .MaximumLength(50).WithMessage("El estado no puede exceder los 50 caracteres")
                .When(p => p.Status != null);

            RuleFor(p => p.CommunityString)
                .MaximumLength(50).WithMessage("La cadena de comunidad no puede exceder los 50 caracteres")
                .When(p => p.CommunityString != null);

            RuleFor(p => p.SnmpPort)
                .InclusiveBetween(1, 65535).When(p => p.SnmpPort.HasValue)
                .WithMessage("El puerto SNMP debe estar entre 1 y 65535");

            RuleFor(p => p.Notes)
                .MaximumLength(500).WithMessage("Las notas no pueden exceder los 500 caracteres")
                .When(p => p.Notes != null);

            RuleFor(p => p.LastError)
                .MaximumLength(1000).WithMessage("El último error no puede exceder los 1000 caracteres")
                .When(p => p.LastError != null);
        }
    }
}
