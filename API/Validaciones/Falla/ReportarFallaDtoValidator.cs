using Biblioteca.DTO.Falla;
using FluentValidation;

namespace API.Validaciones.Falla
{
    public class ReportarFallaDtoValidator : AbstractValidator<ReportarFallaDTO>
    {
        public static readonly string[] SeveridadesPermitidas = { "Informativo", "Alerta", "Crítico" };

        public ReportarFallaDtoValidator()
        {
            RuleFor(x => x.VehiculoId)
                .GreaterThan(0).WithMessage("El vehículo es obligatorio.");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MinimumLength(5).WithMessage("La descripción debe tener al menos 5 caracteres.")
                .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.");

            RuleFor(x => x.Severidad)
                .NotEmpty().WithMessage("La severidad es obligatoria.")
                .Must(s => SeveridadesPermitidas.Contains(s))
                .WithMessage($"La severidad debe ser una de: {string.Join(", ", SeveridadesPermitidas)}.");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria.")
                .Must(f => f.Date <= DateTime.Today).WithMessage("La fecha no puede ser futura.");
        }
    }
}