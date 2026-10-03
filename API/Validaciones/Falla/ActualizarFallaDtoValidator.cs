using Biblioteca.DTO.Falla;
using FluentValidation;

namespace API.Validaciones.Falla
{
    public class ActualizarFallaDtoValidator : AbstractValidator<ActualizarFallaDTO>
    {
        public ActualizarFallaDtoValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MinimumLength(5).WithMessage("La descripción debe tener al menos 5 caracteres.")
                .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

            RuleFor(x => x.Severidad)
                .NotEmpty().WithMessage("La severidad es obligatoria.")
                .Must(s => ReportarFallaDtoValidator.SeveridadesPermitidas.Contains(s))
                .WithMessage($"La severidad debe ser una de: {string.Join(", ", ReportarFallaDtoValidator.SeveridadesPermitidas)}.");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria.")
                .Must(f => f.Date <= DateTime.Today).WithMessage("La fecha no puede ser futura.");
        }
    }
}
