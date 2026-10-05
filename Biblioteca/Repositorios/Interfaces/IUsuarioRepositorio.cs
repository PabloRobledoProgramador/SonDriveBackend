using Biblioteca.Modelo;

namespace Biblioteca.Repositorios.Interfaces
{
    // Solo acceso a datos. Las validaciones y reglas de negocio viven en el servicio.
    public interface IUsuarioRepositorio
    {
        Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default);
        Task<Usuario?> ObtenerPorIdAsync(long id, bool incluirVehiculos = false, CancellationToken ct = default);
        Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
        Task<bool> ExisteCorreoAsync(string correo, long? excluirId = null, CancellationToken ct = default);
        Task<bool> ExisteRolAsync(string rol, CancellationToken ct = default);

        Task<Usuario> CrearAsync(Usuario usuario, CancellationToken ct = default);
        Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken ct = default);
        Task<bool> EliminarAsync(long id, CancellationToken ct = default);
    }
}