namespace Biblioteca.DTO.Ocupacion
{
    public class CrearOcupacionDTO
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string? Direccion { get; set; }
    }
}