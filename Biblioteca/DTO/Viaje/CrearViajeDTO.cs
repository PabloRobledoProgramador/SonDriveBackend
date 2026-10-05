namespace Biblioteca.DTO.Viaje
{
    public class CrearViajeDTO
    {
        public long CiudadOrigenId { get; set; }
        public long CiudadDestinoId { get; set; }
        public long IdVehiculo { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public decimal Precio { get; set; }
    }
}