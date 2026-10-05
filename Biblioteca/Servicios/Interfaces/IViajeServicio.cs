using Biblioteca.DTO.Viaje;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IViajeServicio
    {
        Task<IEnumerable<ViajeResumenDTO>> BuscarAsync(BuscarViajeDTO filtro, CancellationToken ct = default);
        Task<IEnumerable<ViajeResumenDTO>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default);
        Task<ViajeResponseDTO> ObtenerPorIdAsync(long id, CancellationToken ct = default);
        Task<ViajeResumenDTO> CrearAsync(CrearViajeDTO dto, CancellationToken ct = default);
        Task<ViajeResumenDTO> ActualizarAsync(long id, CrearViajeDTO dto, CancellationToken ct = default);
        Task EliminarAsync(long id, CancellationToken ct = default);
    }
}