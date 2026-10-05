using Biblioteca.Modelo;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface IViajeRepositorio
    {
        Task<IReadOnlyList<Viaje>> BuscarAsync(long? origenId, long? destinoId, DateTime? fecha, TimeSpan? hora, CancellationToken ct = default);
        Task<IReadOnlyList<Viaje>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default);
        Task<Viaje?> ObtenerPorIdAsync(long id, bool incluirDetalle = false, CancellationToken ct = default);
        Task<bool> ExisteViajeVehiculoAsync(long idVehiculo, DateTime fechaSalida, long? excluirId = null, CancellationToken ct = default);
        Task<bool> TieneOcupacionesAsync(long id, CancellationToken ct = default);
        Task<Viaje> CrearAsync(Viaje viaje, CancellationToken ct = default);
        Task<Viaje> ActualizarAsync(Viaje viaje, CancellationToken ct = default);
        Task<bool> EliminarAsync(long id, CancellationToken ct = default);
    }
}