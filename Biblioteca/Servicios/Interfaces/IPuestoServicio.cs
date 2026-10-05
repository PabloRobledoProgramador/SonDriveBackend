namespace Biblioteca.Servicios.Interfaces
{
    public interface IPuestoServicio
    {
        // Genera los puestos que le falten al vehículo. Es idempotente.
        Task AsegurarMapaAsync(long idVehiculo, CancellationToken ct = default);
    }
}