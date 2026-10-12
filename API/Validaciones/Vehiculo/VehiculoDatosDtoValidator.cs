using Biblioteca.DTO.Vehiculo;
using FluentValidation;

namespace API.Validaciones.Vehiculo
{
    public class VehiculoDatosDtoValidator : AbstractValidator<VehiculoDatosDTO>
    {
        public VehiculoDatosDtoValidator()
        {
            RuleFor(x => x.IdUsuario)
                .GreaterThan(0).WithMessage("El conductor es obligatorio.");

            RuleFor(x => x.Placa)
                .NotEmpty().WithMessage("La placa es obligatoria.")
                .Matches(@"^[A-Za-z]{3}\s?\d{3}$").WithMessage("La placa debe tener 3 letras y 3 números, por ejemplo SXK 204.");

            RuleFor(x => x.Modelo)
                .NotEmpty().WithMessage("El modelo es obligatorio.")
                .MaximumLength(100).WithMessage("El modelo no puede superar los 100 caracteres.");

            // Se detiene en la primera regla que falla. Antes un When al final condicionaba toda la
            // cadena, también el NotEmpty, y un año vacío pasaba la validación.
            RuleFor(x => x.AnnoModelo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El año del modelo es obligatorio.")
                .Matches(@"^\d{4}$").WithMessage("El año debe tener 4 dígitos.")
                .Must(a => int.TryParse(a, out var anno) && anno >= 1990 && anno <= DateTime.Today.Year + 1)
                .WithMessage("El año del modelo no es válido.");

            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("El tipo de vehículo es obligatorio.")
                .MaximumLength(50).WithMessage("El tipo no puede superar los 50 caracteres.");

            RuleFor(x => x.Kilometraje)
                .GreaterThanOrEqualTo(0).WithMessage("El kilometraje no puede ser negativo.");

            RuleFor(x => x.FechaUltimoServicio)
                .Must(f => f!.Value.Date <= DateTime.Today).WithMessage("La fecha del último servicio no puede ser futura.")
                .When(x => x.FechaUltimoServicio.HasValue);

            RuleFor(x => x.VencimientoSoat)
                .NotEmpty().WithMessage("El vencimiento del SOAT es obligatorio.");

            RuleFor(x => x.VencimientoTecnomecanica)
                .NotEmpty().WithMessage("El vencimiento de la tecnomecánica es obligatorio.");

            RuleFor(x => x.VencimientoLicencia)
                .NotEmpty().WithMessage("El vencimiento de la licencia es obligatorio.");
        }
    }
}