namespace Biblioteca.DTO.InicioSesion
{
    // Recibe credenciales
    public class InicioSesionDTO
    {
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
    }
}
