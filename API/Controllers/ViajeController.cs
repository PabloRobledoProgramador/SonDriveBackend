using API.Seguridad;
using Biblioteca.DTO.Viaje;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ViajeController : ControllerBase
    {
        private readonly IViajeServicio _servicio;
        private readonly IValidator<CrearViajeDTO> _validator;

        public ViajeController(IViajeServicio servicio, IValidator<CrearViajeDTO> validator)
        {
            _servicio = servicio;
            _validator = validator;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViajeResumenDTO>>> Buscar([FromQuery] BuscarViajeDTO filtro, CancellationToken ct)
        {
            var viajes = await _servicio.BuscarAsync(filtro, ct);
            return Ok(viajes);
        }

        [HttpGet("usuario/{idUsuario:long}")]
        public async Task<ActionResult<IEnumerable<ViajeResumenDTO>>> ListarPorUsuario(long idUsuario, CancellationToken ct)
        {
            if (!User.PuedeAccederAUsuario(idUsuario))
                return Forbid();

            var viajes = await _servicio.ListarPorUsuarioAsync(idUsuario, ct);
            return Ok(viajes);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ViajeResponseDTO>> ObtenerPorId(long id, CancellationToken ct)
        {
            var viaje = await _servicio.ObtenerPorIdAsync(id, ct);
            return Ok(viaje);
        }

        [HttpPost]
        public async Task<ActionResult<ViajeResumenDTO>> Crear([FromBody] CrearViajeDTO dto, CancellationToken ct)
        {
            var validacion = await _validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var creado = await _servicio.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<ViajeResumenDTO>> Actualizar(long id, [FromBody] CrearViajeDTO dto, CancellationToken ct)
        {
            var validacion = await _validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var actualizado = await _servicio.ActualizarAsync(id, dto, ct);
            return Ok(actualizado);
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