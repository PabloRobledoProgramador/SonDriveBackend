using Biblioteca.Seguridad;
using System.Security.Claims;

namespace API.Seguridad
{
    // Utilidades para leer los datos del usuario autenticado desde los controladores (User)
    public static class ExtensionesUsuarioActual
    {
        public static long ObtenerIdUsuario(this ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirstValue(NombresClaims.IdUsuario);
            return long.TryParse(valor, out var id)
                ? id
                : throw new InvalidOperationException("El token no contiene el id del usuario.");
        }

        public static bool EsAdministrador(this ClaimsPrincipal usuario) =>
            usuario.IsInRole(Roles.Administrador);

        // El administrador puede acceder a cualquier usuario; los demás solo a sí mismos
        public static bool PuedeAccederAUsuario(this ClaimsPrincipal usuario, long idUsuario) =>
            usuario.EsAdministrador() || usuario.ObtenerIdUsuario() == idUsuario;
    }
}
