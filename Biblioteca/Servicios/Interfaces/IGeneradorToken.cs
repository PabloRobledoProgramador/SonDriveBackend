using Biblioteca.Modelo;

namespace Biblioteca.Servicios.Interfaces
{
    // La implementación vive en la capa API (depende de la configuración y de las librerías de JWT)
    public interface IGeneradorToken
    {
        (string Token, DateTime Expiracion) Generar(Usuario usuario);
    }
}
