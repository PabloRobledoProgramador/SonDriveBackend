using Biblioteca.DTO.Ciudad;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class CiudadServicio : ICiudadServicio
    {
        private readonly ICiudadRepositorio _repositorio;

        public CiudadServicio(ICiudadRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<IEnumerable<CiudadResponseDTO>> ListarAsync(CancellationToken ct = default)
        {
            var ciudades = await _repositorio.ListarAsync(ct);

            return ciudades.Select(c => new CiudadResponseDTO
            {
                Id = c.Id,
                Nombre = c.Nombre
            });
        }
    }
}