namespace Biblioteca.DTO.Viaje
{
    // Sale: RESUMEN. Tarjetas de "Viajes disponibles" (resultado de Buscar viaje) y lista de "Mis viajes"
    public class ViajeResumenDTO
    {
        public long Id { get; set; }
        public DateTime FechaSalida { get; set; }
        public decimal Precio { get; set; }
        public long CiudadOrigenId { get; set; }
        public string CiudadOrigen { get; set; } = string.Empty;
        public long CiudadDestinoId { get; set; }
        public string CiudadDestino { get; set; } = string.Empty;
        public long IdVehiculo { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public string? FotoConductor { get; set; }
        public int CuposLibres { get; set; }
        public int CajuelasLibres { get; set; }
    }
}