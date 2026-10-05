using Biblioteca.Modelo;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface IOcupacionRepositorio
    {
        Task<IReadOnlyList<Ocupacion>> ListarPorViajeAsync(long idViaje, CancellationToken ct = default);
        Task<Ocupacion?> ObtenerAsync(long idViaje, long idPuesto, CancellationToken ct = default);
        Task<Ocupacion> CrearAsync(Ocupacion ocupacion, CancellationToken ct = default);
        Task ActualizarPasajeroAsync(Pasajero pasajero, CancellationToken ct = default);
        Task EliminarAsync(long idOcupacion, CancellationToken ct = default);
    }
}