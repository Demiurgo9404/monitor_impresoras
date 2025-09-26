using FluentValidation;
using MonitorImpresoras.Application.DTOs;

namespace MonitorImpresoras.API.Validations
{
    public class CreatePrinterDTOValidator : AbstractValidator<CreatePrinterDTO>
    {
        public CreatePrinterDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la impresora es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("El modelo es requerido")
                .MaximumLength(50).WithMessage("El modelo no puede exceder 50 caracteres");

            RuleFor(x => x.IpAddress)
                .NotEmpty().WithMessage("La dirección IP es requerida")
                .Matches(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
                .WithMessage("La dirección IP no tiene un formato válido");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("La ubicación es requerida")
                .MaximumLength(100).WithMessage("La ubicación no puede exceder 100 caracteres");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres");

            RuleFor(x => x.SerialNumber)
                .MaximumLength(50).WithMessage("El número de serie no puede exceder 50 caracteres");
        }
    }

    public class UpdatePrinterDTOValidator : AbstractValidator<UpdatePrinterDTO>
    {
        public UpdatePrinterDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre de la impresora es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
                .When(x => x.Name != null);

            RuleFor(x => x.Model)
                .MaximumLength(50).WithMessage("El modelo no puede exceder 50 caracteres")
                .When(x => x.Model != null);

            RuleFor(x => x.IpAddress)
                .Matches(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
                .WithMessage("La dirección IP no tiene un formato válido")
                .When(x => !string.IsNullOrEmpty(x.IpAddress));

            RuleFor(x => x.Location)
                .MaximumLength(100).WithMessage("La ubicación no puede exceder 100 caracteres")
                .When(x => x.Location != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => x.Description != null);
        }
    }
}
