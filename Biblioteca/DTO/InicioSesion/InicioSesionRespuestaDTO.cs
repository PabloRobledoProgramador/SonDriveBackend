using Biblioteca.DTO.Usuario;

namespace Biblioteca.DTO.InicioSesion
{
    // Respuesta de un inicio de sesión exitoso
    public class InicioSesionRespuestaDTO
    {
        public string Token { get; set; } = string.Empty;

        // Fecha y hora (UTC) en la que el token deja de ser válido
        public DateTime Expiracion { get; set; }

        public UsuarioDto Usuario { get; set; } = new();
    }
}
