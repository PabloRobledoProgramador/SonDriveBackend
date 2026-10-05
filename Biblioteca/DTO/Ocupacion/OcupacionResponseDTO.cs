namespace Biblioteca.DTO.Ocupacion
{
    public class OcupacionResponseDTO
    {
        public long Id { get; set; }
        public long IdViaje { get; set; }
        public long IdPuesto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public PasajeroOcupacionDTO Pasajero { get; set; } = new();
    }
}