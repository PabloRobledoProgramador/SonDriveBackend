using Biblioteca.DTO.Ocupacion;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/viaje/{idViaje:long}/ocupaciones")]
    public class OcupacionController : ControllerBase
    {
        private readonly IOcupacionServicio _servicio;
        private readonly IValidator<CrearOcupacionDTO> _validator;

        public OcupacionController(IOcupacionServicio servicio, IValidator<CrearOcupacionDTO> validator)
        {
            _servicio = servicio;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OcupacionResponseDTO>>> Listar(long idViaje, CancellationToken ct)
        {
            var ocupaciones = await _servicio.ListarPorViajeAsync(idViaje, ct);
            return Ok(ocupaciones);
        }

        [HttpPost("puesto/{idPuesto:long}")]
        public async Task<ActionResult<OcupacionResponseDTO>> Ocupar(long idViaje, long idPuesto, [FromBody] CrearOcupacionDTO dto, CancellationToken ct)
        {
            var validacion = await _validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var ocupacion = await _servicio.OcuparAsync(idViaje, idPuesto, dto, ct);
            return Ok(ocupacion);
        }

        // Edita los datos del pasajero sin liberar el puesto.
        [HttpPut("puesto/{idPuesto:long}")]
        public async Task<ActionResult<OcupacionResponseDTO>> ActualizarPasajero(long idViaje, long idPuesto, [FromBody] CrearOcupacionDTO dto, CancellationToken ct)
        {
            var validacion = await _validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var ocupacion = await _servicio.ActualizarPasajeroAsync(idViaje, idPuesto, dto, ct);
            return Ok(ocupacion);
        }

        [HttpDelete("puesto/{idPuesto:long}")]
        public async Task<IActionResult> Liberar(long idViaje, long idPuesto, CancellationToken ct)
        {
            await _servicio.LiberarAsync(idViaje, idPuesto, ct);
            return NoContent();
        }

        private ActionResult ProblemaDeValidacion(ValidationResult resultado)
        {
            var problema = new ValidationProblemDetails(resultado.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest
            };
            return ValidationProblem(problema);
        }
    }
}