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
    }
}
