using Biblioteca.Modelo;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Pruebas.Infraestructura;

// Doble de IGeneradorToken con valores fijos: las pruebas del servicio no dependen de JWT ni de la hora real
public sealed class GeneradorTokenFalso : IGeneradorToken
{
    public const string Token = "token-de-prueba";
    public static readonly DateTime Expiracion = new(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public Usuario? UltimoUsuario { get; private set; }

    public (string Token, DateTime Expiracion) Generar(Usuario usuario)
    {
        UltimoUsuario = usuario;
        return (Token, Expiracion);
    }
}
