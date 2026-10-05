using Biblioteca.Modelo;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface IPuestoRepositorio
    {
        Task<IReadOnlyList<Puesto>> ListarPorVehiculoAsync(long idVehiculo, CancellationToken ct = default);
        Task<CodigoPuesto> ObtenerOCrearCodigoAsync(string descripcion, CancellationToken ct = default);
        Task<TipoPuesto> ObtenerOCrearTipoAsync(string descripcion, CancellationToken ct = default);
        Task AgregarRangoAsync(IEnumerable<Puesto> puestos, CancellationToken ct = default);
    }
}