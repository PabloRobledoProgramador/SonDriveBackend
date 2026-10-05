namespace Biblioteca.DTO.Viaje
{
    public class PuestoViajeDTO
    {
        public long IdPuesto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public bool EsConductor { get; set; }
        public bool Ocupado { get; set; }
    }
}