using FluentValidation;

namespace API.Validaciones.Usuario
{
    public class UsuarioCambiarClaveDtoValidator : AbstractValidator<Biblioteca.DTO.Usuario.UsuarioCambiarClaveDto>
    {
        public UsuarioCambiarClaveDtoValidator()
        {
            RuleFor(x => x.ClaveActual)
                .NotEmpty().WithMessage("La clave actual es obligatoria.");

            RuleFor(x => x.ClaveNueva)
                .NotEmpty().WithMessage("La clave nueva es obligatoria.")
                .MinimumLength(8).WithMessage("La clave nueva debe tener al menos 8 caracteres.")
                .MaximumLength(72).WithMessage("La clave nueva no puede superar los 72 caracteres.")
                .NotEqual(x => x.ClaveActual).WithMessage("La clave nueva debe ser diferente a la actual.");
        }
    }
}
