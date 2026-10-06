using Microsoft.IdentityModel.JsonWebTokens;

namespace API.Seguridad
{
    // Nombres de los claims que viajan dentro del token
    public static class NombresClaims
    {
        public const string IdUsuario = JwtRegisteredClaimNames.Sub;
        public const string Correo = JwtRegisteredClaimNames.Email;
        public const string Nombre = "nombre";
        public const string Rol = "rol";
    }
}
