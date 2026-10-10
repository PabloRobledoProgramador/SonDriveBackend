using Biblioteca.DTO.Ruta;
using Biblioteca.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RutaController : ControllerBase
    {
        private readonly IRutaServicio _servicio;

        public RutaController(IRutaServicio servicio)
        {
            _servicio = servicio;
        }

        // GET api/ruta
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RutaResponseDTO>>> Listar(CancellationToken ct)
        {
            var rutas = await _servicio.ListarAsync(ct);
            return Ok(rutas);
        }
    }
}
