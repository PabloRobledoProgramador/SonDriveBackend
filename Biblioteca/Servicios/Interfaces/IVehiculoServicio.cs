using Biblioteca.DTO.Vehiculo;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IVehiculoServicio
    {
        Task<IEnumerable<VehiculoResponseDTO>> ListarAsync(CancellationToken ct = default);
        Task<IEnumerable<VehiculoResponseDTO>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default);
        Task<VehiculoResponseDTO> ObtenerPorIdAsync(long id, CancellationToken ct = default);
        Task<VehiculoResponseDTO> CrearAsync(VehiculoDatosDTO dto, CancellationToken ct = default);
        Task<VehiculoResponseDTO> ActualizarAsync(long id, VehiculoDatosDTO dto, CancellationToken ct = default);
        Task EliminarAsync(long id, CancellationToken ct = default);
    }
}