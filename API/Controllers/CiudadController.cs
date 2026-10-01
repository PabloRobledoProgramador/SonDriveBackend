using Biblioteca.DTO.Ciudad;
using Biblioteca.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CiudadController : ControllerBase
    {
        private readonly ICiudadServicio _servicio;

        public CiudadController(ICiudadServicio servicio)
        {
            _servicio = servicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CiudadResponseDTO>>> Listar(CancellationToken ct)
        {
            var ciudades = await _servicio.ListarAsync(ct);
            return Ok(ciudades);
        }
    }
}