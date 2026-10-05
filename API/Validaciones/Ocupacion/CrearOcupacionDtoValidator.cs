using System.Text.RegularExpressions;
using Biblioteca.DTO.Ocupacion;
using FluentValidation;

namespace API.Validaciones.Ocupacion
{
    public class CrearOcupacionDtoValidator : AbstractValidator<CrearOcupacionDTO>
    {
        // Opcional "+" al inicio, luego dígitos y espacios (ej: "3001234567", "+57 300 123 4567").
        private static readonly Regex CelularRegex = new(@"^\+?[0-9][0-9 ]{6,18}$", RegexOptions.Compiled);

        public CrearOcupacionDtoValidator()
        {
            RuleFor(x => x.NombreCompleto)
                .NotEmpty().WithMessage("El nombre del pasajero es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("El celular del pasajero es obligatorio.")
                .MaximumLength(20).WithMessage("El celular no puede superar 20 caracteres.")
                .Must(c => CelularRegex.IsMatch(c.Trim()))
                    .WithMessage("El celular solo puede tener números (y un + al inicio), entre 7 y 20 caracteres.")
                    .When(x => !string.IsNullOrWhiteSpace(x.Celular));

            RuleFor(x => x.Direccion)
                .MaximumLength(250).WithMessage("La dirección no puede superar 250 caracteres.");
        }
    }
}