namespace Biblioteca.DTO.Usuario
{
    // Datos que la administradora puede corregir de un conductor. No incluye Estado, Rol ni Clave:
    // así la edición no puede cambiarlos.
    public class ConductorActualizarDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string? Foto { get; set; }
    }
}
