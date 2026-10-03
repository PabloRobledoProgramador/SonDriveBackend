using Biblioteca.Modelo;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface IVehiculoRepositorio
    {
        Task<IReadOnlyList<Vehiculo>> ListarAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Vehiculo>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default);
        Task<Vehiculo?> ObtenerPorIdAsync(long id, bool incluirDetalle = false, CancellationToken ct = default);
        Task<bool> ExistePlacaAsync(string placa, long? excluirId = null, CancellationToken ct = default);
        Task<bool> TieneViajesAsync(long id, CancellationToken ct = default);
        Task<Vehiculo> CrearAsync(Vehiculo vehiculo, CancellationToken ct = default);
        Task<Vehiculo> ActualizarAsync(Vehiculo vehiculo, CancellationToken ct = default);
        Task<bool> EliminarAsync(long id, CancellationToken ct = default);
    }
}