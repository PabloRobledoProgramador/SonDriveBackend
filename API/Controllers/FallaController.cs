using Biblioteca.DTO.Falla;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FallaController : ControllerBase
    {
        private readonly IFallaServicio _servicio;
        private readonly IValidator<ReportarFallaDTO> _validatorReportar;
        private readonly IValidator<ActualizarFallaDTO> _validatorActualizar;

        public FallaController(
            IFallaServicio servicio,
            IValidator<ReportarFallaDTO> validatorReportar,
            IValidator<ActualizarFallaDTO> validatorActualizar)
        {
            _servicio = servicio;
            _validatorReportar = validatorReportar;
            _validatorActualizar = validatorActualizar;
        }

        [HttpGet("vehiculo/{idVehiculo:long}")]
        public async Task<ActionResult<IEnumerable<FallaResponseDTO>>> ListarPorVehiculo(long idVehiculo, CancellationToken ct)
        {
            var fallas = await _servicio.ListarPorVehiculoAsync(idVehiculo, ct);
            return Ok(fallas);
        }

        [HttpPost]
        public async Task<ActionResult<FallaResponseDTO>> Reportar([FromBody] ReportarFallaDTO dto, CancellationToken ct)
        {
            var validacion = await _validatorReportar.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var creada = await _servicio.ReportarAsync(dto, ct);
            return CreatedAtAction(nameof(ListarPorVehiculo), new { idVehiculo = creada.VehiculoId }, creada);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<FallaResponseDTO>> Actualizar(long id, [FromBody] ActualizarFallaDTO dto, CancellationToken ct)
        {
            var validacion = await _validatorActualizar.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var actualizada = await _servicio.ActualizarAsync(id, dto, ct);
            return Ok(actualizada);
        }

        [HttpPatch("{id:long}/resuelta")]
        public async Task<ActionResult<FallaResponseDTO>> CambiarResuelta(long id, [FromQuery] bool resuelta, CancellationToken ct)
        {
            var actualizada = await _servicio.CambiarResueltaAsync(id, resuelta, ct);
            return Ok(actualizada);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
        {
            await _servicio.EliminarAsync(id, ct);
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