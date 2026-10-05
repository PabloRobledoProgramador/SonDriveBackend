using Biblioteca.DTO.Viaje;
using FluentValidation;

namespace API.Validaciones.Viaje
{
    public class CrearViajeDtoValidator : AbstractValidator<CrearViajeDTO>
    {
        public CrearViajeDtoValidator()
        {
            RuleFor(x => x.CiudadOrigenId)
                .GreaterThan(0).WithMessage("El origen es obligatorio.");

            RuleFor(x => x.CiudadDestinoId)
                .GreaterThan(0).WithMessage("El destino es obligatorio.");

            RuleFor(x => x.CiudadDestinoId)
                .NotEqual(x => x.CiudadOrigenId).WithMessage("El destino debe ser distinto al origen.")
                .When(x => x.CiudadOrigenId > 0 && x.CiudadDestinoId > 0);

            RuleFor(x => x.IdVehiculo)
                .GreaterThan(0).WithMessage("El vehículo es obligatorio.");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha es obligatoria.");

            RuleFor(x => x.Hora)
                .Must(h => h >= TimeSpan.Zero && h < TimeSpan.FromDays(1)).WithMessage("La hora no es válida.");

            RuleFor(x => x.Fecha)
                .Must((x, fecha) => fecha.Date + x.Hora > DateTime.Now).WithMessage("La fecha y hora de salida deben ser futuras.")
                .When(x => x.Fecha != default && x.Hora >= TimeSpan.Zero && x.Hora < TimeSpan.FromDays(1));

            RuleFor(x => x.Precio)
                .GreaterThan(0).WithMessage("El precio debe ser mayor a cero.");
        }
    }
}