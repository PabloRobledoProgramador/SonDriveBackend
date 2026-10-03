using Biblioteca.Modelo;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface IFallaRepositorio
    {
        Task<IReadOnlyList<Falla>> ListarPorVehiculoAsync(long idVehiculo, CancellationToken ct = default);
        Task<Falla?> ObtenerPorIdAsync(long id, CancellationToken ct = default);
        Task<Falla> CrearAsync(Falla falla, CancellationToken ct = default);
        Task<Falla> ActualizarAsync(Falla falla, CancellationToken ct = default);
        Task<bool> EliminarAsync(long id, CancellationToken ct = default);
    }
}