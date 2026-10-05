using Biblioteca.DTO.InicioSesion;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IAutenticacionServicio
    {
        Task<InicioSesionRespuestaDTO> IniciarSesionAsync(InicioSesionDTO dto, CancellationToken ct = default);
    }
}
