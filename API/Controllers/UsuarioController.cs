using Biblioteca.DTO.Usuario;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioServicio _servicio;
        private readonly IValidator<UsuarioCrearDto> _validadorCrear;
        private readonly IValidator<UsuarioActualizarDto> _validadorActualizar;
        private readonly IValidator<UsuarioCambiarClaveDto> _validadorClave;

        public UsuarioController(
            IUsuarioServicio servicio,
            IValidator<UsuarioCrearDto> validadorCrear,
            IValidator<UsuarioActualizarDto> validadorActualizar,
            IValidator<UsuarioCambiarClaveDto> validadorClave)
        {
            _servicio = servicio;
            _validadorCrear = validadorCrear;
            _validadorActualizar = validadorActualizar;
            _validadorClave = validadorClave;
        }

        // GET api/usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> Listar(CancellationToken ct)
        {
            var usuarios = await _servicio.ListarAsync(ct);
            return Ok(usuarios);
        }

        // GET api/usuario/5
        [HttpGet("{id:long}")]
        public async Task<ActionResult<UsuarioDto>> ObtenerPorId(long id, CancellationToken ct)
        {
            var usuario = await _servicio.ObtenerPorIdAsync(id, ct);
            return Ok(usuario);
        }

        // POST api/usuario
        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> Crear([FromBody] UsuarioCrearDto dto, CancellationToken ct)
        {
            var validacion = await _validadorCrear.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var creado = await _servicio.CrearAsync(dto, ct);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
        }

        // PUT api/usuario/5
        [HttpPut("{id:long}")]
        public async Task<ActionResult<UsuarioDto>> Actualizar(long id, [FromBody] UsuarioActualizarDto dto, CancellationToken ct)
        {
            var validacion = await _validadorActualizar.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var actualizado = await _servicio.ActualizarAsync(id, dto, ct);
            return Ok(actualizado);
        }

        // PUT api/usuario/5/clave
        [HttpPut("{id:long}/clave")]
        public async Task<IActionResult> CambiarClave(long id, [FromBody] UsuarioCambiarClaveDto dto, CancellationToken ct)
        {
            var validacion = await _validadorClave.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            await _servicio.CambiarClaveAsync(id, dto, ct);
            return NoContent();
        }

        // PATCH api/usuario/5/estado?estado=false
        [HttpPatch("{id:long}/estado")]
        public async Task<IActionResult> CambiarEstado(long id, [FromQuery] bool estado, CancellationToken ct)
        {
            await _servicio.CambiarEstadoAsync(id, estado, ct);
            return NoContent();
        }

        // DELETE api/usuario/5
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Eliminar(long id, CancellationToken ct)
        {
            await _servicio.EliminarAsync(id, ct);
            return NoContent();
        }

        // Convierte el resultado de FluentValidation en un 400 con formato estándar
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