using Biblioteca.DTO.Ciudad;

namespace Biblioteca.Servicios.Interfaces
{
    public interface ICiudadServicio
    {
        Task<IEnumerable<CiudadResponseDTO>> ListarAsync(CancellationToken ct = default);
    }
}