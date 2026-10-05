using API.Seguridad;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.DTO.Usuario;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacionController : ControllerBase
    {
        private readonly IAutenticacionServicio _servicio;
        private readonly IUsuarioServicio _usuarioServicio;
        private readonly IValidator<InicioSesionDTO> _validador;

        public AutenticacionController(
            IAutenticacionServicio servicio,
            IUsuarioServicio usuarioServicio,
            IValidator<InicioSesionDTO> validador)
        {
            _servicio = servicio;
            _usuarioServicio = usuarioServicio;
            _validador = validador;
        }

        // POST api/autenticacion/iniciar-sesion
        [AllowAnonymous]
        [HttpPost("iniciar-sesion")]
        public async Task<ActionResult<InicioSesionRespuestaDTO>> IniciarSesion([FromBody] InicioSesionDTO dto, CancellationToken ct)
        {
            var validacion = await _validador.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
                return ProblemaDeValidacion(validacion);

            var respuesta = await _servicio.IniciarSesionAsync(dto, ct);
            return Ok(respuesta);
        }

        // GET api/autenticacion/perfil (datos del usuario dueño del token)
        [HttpGet("perfil")]
        public async Task<ActionResult<UsuarioDto>> Perfil(CancellationToken ct)
        {
            var usuario = await _usuarioServicio.ObtenerPorIdAsync(User.ObtenerIdUsuario(), ct);
            return Ok(usuario);
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
