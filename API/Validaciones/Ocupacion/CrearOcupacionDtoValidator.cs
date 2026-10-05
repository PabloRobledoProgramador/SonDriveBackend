using Biblioteca.DTO.Ocupacion;
using FluentValidation;

namespace API.Validaciones.Ocupacion
{
    public class CrearOcupacionDtoValidator : AbstractValidator<CrearOcupacionDTO>
    {
        public CrearOcupacionDtoValidator()
        {
            RuleFor(x => x.NombreCompleto)
                .NotEmpty().WithMessage("El nombre del pasajero es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

            RuleFor(x => x.Celular)
                .NotEmpty().WithMessage("El celular del pasajero es obligatorio.")
                .MaximumLength(20).WithMessage("El celular no puede superar 20 caracteres.");

            RuleFor(x => x.Direccion)
                .MaximumLength(250).WithMessage("La dirección no puede superar 250 caracteres.");
        }
    }
}