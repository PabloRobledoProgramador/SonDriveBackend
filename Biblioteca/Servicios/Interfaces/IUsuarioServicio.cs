using Biblioteca.DTO.Usuario;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IUsuarioServicio
    {
        Task<IEnumerable<UsuarioDto>> ListarAsync(CancellationToken ct = default);
        Task<UsuarioDto> ObtenerPorIdAsync(long id, CancellationToken ct = default);
        Task<UsuarioDto> CrearAsync(UsuarioCrearDto dto, CancellationToken ct = default);
        Task<UsuarioDto> ActualizarAsync(long id, UsuarioActualizarDto dto, CancellationToken ct = default);
        Task CambiarClaveAsync(long id, UsuarioCambiarClaveDto dto, CancellationToken ct = default);
        Task CambiarEstadoAsync(long id, bool estado, CancellationToken ct = default);
        Task EliminarAsync(long id, CancellationToken ct = default);

        // Indica si hay al menos un usuario con el rol dado (usado al arrancar para crear el administrador inicial)
        Task<bool> ExisteRolAsync(string rol, CancellationToken ct = default);
    }
}
