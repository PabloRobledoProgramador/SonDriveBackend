using Biblioteca.DTO.Falla;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IFallaServicio
    {
        Task<IEnumerable<FallaResponseDTO>> ListarPorVehiculoAsync(long idVehiculo, CancellationToken ct = default);
        Task<FallaResponseDTO> ReportarAsync(ReportarFallaDTO dto, CancellationToken ct = default);
        Task<FallaResponseDTO> ActualizarAsync(long id, ActualizarFallaDTO dto, CancellationToken ct = default);
        Task<FallaResponseDTO> CambiarResueltaAsync(long id, bool resuelta, CancellationToken ct = default);
        Task EliminarAsync(long id, CancellationToken ct = default);
    }
}