using AutoMapper;
using Biblioteca.DTO.Usuario;
using Biblioteca.Excepciones;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class UsuarioServicio : IUsuarioServicio
    {
        private readonly IUsuarioRepositorio _repositorio;
        private readonly IMapper _mapper;

        public UsuarioServicio(IUsuarioRepositorio repositorio, IMapper mapper)
        {
            _repositorio = repositorio;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UsuarioDto>> ListarAsync(CancellationToken ct = default)
        {
            var usuarios = await _repositorio.ListarAsync(ct);
            return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
        }

        public async Task<UsuarioDto> ObtenerPorIdAsync(long id, CancellationToken ct = default)
        {
            var usuario = await ObtenerOFallarAsync(id, false, ct);
            return _mapper.Map<UsuarioDto>(usuario);
        }

        public async Task<UsuarioDto> CrearAsync(UsuarioCrearDto dto, CancellationToken ct = default)
        {
            var correo = Normalizar(dto.Correo);

            if (await _repositorio.ExisteCorreoAsync(correo, null, ct))
                throw new ReglaNegocioException($"Ya existe un usuario con el correo '{correo}'.");

            var usuario = _mapper.Map<Usuario>(dto);
            usuario.Correo = correo;
            usuario.Clave = BCrypt.Net.BCrypt.HashPassword(dto.Clave);

            var creado = await _repositorio.CrearAsync(usuario, ct);
            return _mapper.Map<UsuarioDto>(creado);
        }

        public async Task<UsuarioDto> ActualizarAsync(long id, UsuarioActualizarDto dto, CancellationToken ct = default)
        {
            var existente = await ObtenerOFallarAsync(id, false, ct);
            var correo = Normalizar(dto.Correo);

            if (await _repositorio.ExisteCorreoAsync(correo, id, ct))
                throw new ReglaNegocioException($"Ya existe otro usuario con el correo '{correo}'.");

            // Id, Clave y Vehiculos quedan intactos por la configuración del perfil
            _mapper.Map(dto, existente);
            existente.Correo = correo;

            var actualizado = await _repositorio.ActualizarAsync(existente, ct);
            return _mapper.Map<UsuarioDto>(actualizado);
        }

        public async Task CambiarClaveAsync(long id, UsuarioCambiarClaveDto dto, CancellationToken ct = default)
        {
            var existente = await ObtenerOFallarAsync(id, false, ct);

            if (!BCrypt.Net.BCrypt.Verify(dto.ClaveActual, existente.Clave))
                throw new ReglaNegocioException("La clave actual es incorrecta.");

            existente.Clave = BCrypt.Net.BCrypt.HashPassword(dto.ClaveNueva);
            await _repositorio.ActualizarAsync(existente, ct);
        }

        public async Task CambiarEstadoAsync(long id, bool estado, CancellationToken ct = default)
        {
            var existente = await ObtenerOFallarAsync(id, false, ct);

            existente.Estado = estado;
            await _repositorio.ActualizarAsync(existente, ct);
        }

        public async Task EliminarAsync(long id, CancellationToken ct = default)
        {
            var existente = await ObtenerOFallarAsync(id, true, ct);

            if (existente.Vehiculos.Count > 0)
                throw new ReglaNegocioException(
                    "No se puede eliminar el usuario porque tiene vehículos asociados. " +
                    "Desactívalo o elimina primero sus vehículos.");

            await _repositorio.EliminarAsync(id, ct);
        }

        public async Task<bool> ExisteRolAsync(string rol, CancellationToken ct = default)
        {
            return await _repositorio.ExisteRolAsync(rol, ct);
        }

        // ---------- Helpers ----------

        private async Task<Usuario> ObtenerOFallarAsync(long id, bool incluirVehiculos, CancellationToken ct)
        {
            return await _repositorio.ObtenerPorIdAsync(id, incluirVehiculos, ct)
                ?? throw new NoEncontradoException($"No existe un usuario con Id {id}.");
        }

        private static string Normalizar(string correo) =>
            (correo ?? string.Empty).Trim().ToLowerInvariant();
    }
}
