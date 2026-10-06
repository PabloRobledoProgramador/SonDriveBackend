using FluentValidation;
using Biblioteca.DTO.Usuario;
using Biblioteca.Seguridad;

namespace API.Validaciones.Usuario
{
    public class UsuarioCrearDtoValidador : AbstractValidator<UsuarioCrearDto>
    {

        public UsuarioCrearDtoValidador()
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

            RuleFor(x => x.Clave)
                .NotEmpty().WithMessage("La clave es obligatoria.")
                .MinimumLength(8).WithMessage("La clave debe tener al menos 8 caracteres.")
                .MaximumLength(72).WithMessage("La clave no puede superar los 72 caracteres.");

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("El celular es obligatorio.")
                .Matches(@"^3\d{9}$").WithMessage("El celular debe tener 10 dígitos y empezar por 3.");

            RuleFor(x => x.Rol)
                .Must(rol => Roles.Todos.Contains(rol))
                .When(x => !string.IsNullOrWhiteSpace(x.Rol))
                .WithMessage($"El rol debe ser '{Roles.Conductor}' o '{Roles.Administrador}'.");
        }
    }
}
