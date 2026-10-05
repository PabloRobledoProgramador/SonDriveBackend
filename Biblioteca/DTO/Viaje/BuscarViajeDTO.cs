namespace Biblioteca.DTO.Viaje
{
    public class BuscarViajeDTO
    {
        public long? CiudadOrigenId { get; set; }
        public long? CiudadDestinoId { get; set; }
        public DateTime? Fecha { get; set; }
        public TimeSpan? Hora { get; set; }
    }
}