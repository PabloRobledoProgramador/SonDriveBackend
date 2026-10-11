using Biblioteca.DTO.Usuario;
using FluentValidation;

namespace API.Validaciones.Usuario
{
    public class ConductorActualizarDtoValidador : AbstractValidator<ConductorActualizarDto>
    {

        public ConductorActualizarDtoValidador()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("El correo no tiene un formato válido.")
                .MaximumLength(150).WithMessage("El correo no puede superar los 150 caracteres.");

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("El celular es obligatorio.")
                .Matches(@"^3\d{9}$").WithMessage("El celular debe tener 10 dígitos y empezar por 3.");

            RuleFor(x => x.Foto)
                .Must(ValidacionFoto.EsDireccionWebValida).WithMessage(ValidacionFoto.MensajeDireccionInvalida)
                .MaximumLength(ValidacionFoto.LongitudMaxima).WithMessage(ValidacionFoto.MensajeLongitudMaxima)
                .When(x => !string.IsNullOrWhiteSpace(x.Foto));
        }
    }
}
