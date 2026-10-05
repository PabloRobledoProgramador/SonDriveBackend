using AutoMapper;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.DTO.Usuario;
using Biblioteca.Excepciones;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class AutenticacionServicio : IAutenticacionServicio
    {
        private const string MensajeCredencialesInvalidas = "Correo o clave incorrectos.";

        // Hash de relleno: si el correo no existe se verifica contra él para que la respuesta
        // tarde lo mismo que con un correo real y no se pueda adivinar qué correos están registrados.
        private static readonly string HashDeRelleno = BCrypt.Net.BCrypt.HashPassword("clave-de-relleno-sondrive");

        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IGeneradorToken _generadorToken;
        private readonly IMapper _mapper;

        public AutenticacionServicio(IUsuarioRepositorio usuarioRepositorio, IGeneradorToken generadorToken, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _generadorToken = generadorToken;
            _mapper = mapper;
        }

        public async Task<InicioSesionRespuestaDTO> IniciarSesionAsync(InicioSesionDTO dto, CancellationToken ct = default)
        {
            var correo = (dto.Correo ?? string.Empty).Trim().ToLowerInvariant();
            var usuario = await _usuarioRepositorio.ObtenerPorCorreoAsync(correo, ct);

            var claveValida = BCrypt.Net.BCrypt.Verify(dto.Clave, usuario?.Clave ?? HashDeRelleno);

            // Mismo mensaje si falla el correo o la clave: no se revela cuál de los dos es incorrecto
            if (usuario is null || !claveValida)
                throw new CredencialesInvalidasException(MensajeCredencialesInvalidas);

            if (!usuario.Estado)
                throw new CredencialesInvalidasException("El usuario está desactivado. Contacta al administrador.");

            var (token, expiracion) = _generadorToken.Generar(usuario);

            return new InicioSesionRespuestaDTO
            {
                Token = token,
                Expiracion = expiracion,
                Usuario = _mapper.Map<UsuarioDto>(usuario)
            };
        }
    }
}
