using Biblioteca.DTO.Ocupacion;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IOcupacionServicio
    {
        Task<IEnumerable<OcupacionResponseDTO>> ListarPorViajeAsync(long idViaje, CancellationToken ct = default);
        Task<OcupacionResponseDTO> OcuparAsync(long idViaje, long idPuesto, CrearOcupacionDTO dto, CancellationToken ct = default);
        Task LiberarAsync(long idViaje, long idPuesto, CancellationToken ct = default);
    }
}