using Biblioteca.Modelo;
using Biblioteca.Servicios.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace API.Seguridad
{
    public class GeneradorTokenJwt : IGeneradorToken
    {
        private readonly ConfiguracionJwt _configuracion;
        private readonly JsonWebTokenHandler _manejador = new();

        public GeneradorTokenJwt(IOptions<ConfiguracionJwt> configuracion)
        {
            _configuracion = configuracion.Value;
        }

        public (string Token, DateTime Expiracion) Generar(Usuario usuario)
        {
            var expiracion = DateTime.UtcNow.AddMinutes(_configuracion.MinutosExpiracion);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuracion.Emisor,
                Audience = _configuracion.Audiencia,
                Expires = expiracion,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(NombresClaims.IdUsuario, usuario.Id.ToString()),
                    new Claim(NombresClaims.Correo, usuario.Correo),
                    new Claim(NombresClaims.Nombre, $"{usuario.Nombre} {usuario.Apellido}"),
                    new Claim(NombresClaims.Rol, usuario.Rol)
                }),
                SigningCredentials = new SigningCredentials(
                    CrearLlaveFirma(_configuracion.Clave), SecurityAlgorithms.HmacSha256)
            };

            return (_manejador.CreateToken(descriptor), expiracion);
        }

        public static SymmetricSecurityKey CrearLlaveFirma(string clave) =>
            new(Encoding.UTF8.GetBytes(clave));
    }
}
