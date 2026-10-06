using Biblioteca.DTO.InicioSesion;
using FluentValidation;

namespace API.Validaciones.InicioSesion
{
    public class InicioSesionDtoValidador : AbstractValidator<InicioSesionDTO>
    {
        public InicioSesionDtoValidador()
        {
            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("El correo no tiene un formato válido.")
                .MaximumLength(150).WithMessage("El correo no puede superar los 150 caracteres.");

            RuleFor(x => x.Clave)
                .NotEmpty().WithMessage("La clave es obligatoria.")
                .MaximumLength(72).WithMessage("La clave no puede superar los 72 caracteres.");
        }
    }
}
