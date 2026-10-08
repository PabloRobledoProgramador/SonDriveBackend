using API.Seguridad;
using Biblioteca.DTO.Vehiculo;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculoController : ControllerBase
    {
        private readonly IVehiculoServicio _servicio;
        private readonly IValidator<VehiculoDatosDTO> _validator;

        public VehiculoController(IVehiculoServicio servicio, IValidator<VehiculoDatosDTO> validator)
        {
            _servicio = servicio;
            _validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiculoResponseDTO>>> Listar(CancellationToken ct)
        {
            var vehiculos = await _servicio.ListarAsync(ct);
            return Ok(vehiculos);
        }

        [HttpGet("usuario/{idUsuario:long}")]
        public async Task<ActionResult<IEnumerable<VehiculoResponseDTO>>> ListarPorUsuario(long idUsuario, CancellationToken ct)
        {
            if (!User.PuedeAccederAUsuario(idUsuario))
                return Forbid();

            var vehiculos = await _servicio.ListarPorUsuarioAsync(idUsuario, ct);
            return Ok(vehiculos);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<VehiculoResponseDTO>> ObtenerPorId(long id, CancellationToken ct)
        {
            var vehiculo = await _servicio.ObtenerPorIdAsync(id, ct);
            return Ok(vehiculo);
        }

        [HttpPost]
        public async Task<ActionResult<VehiculoResponseDTO>> Crear([FromBody] VehiculoDatosDTO dto, CancellationToken ct)
        {
            var validacion = await _validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var creado = await _servicio.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<VehiculoResponseDTO>> Actualizar(long id, [FromBody] VehiculoDatosDTO dto, CancellationToken ct)
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