namespace Biblioteca.DTO.Vehiculo
{
    // Para Monitoreo en agregar Vehículos
    public class VehiculoDatosDTO
    {
        public long IdUsuario { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string AnnoModelo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Kilometraje { get; set; }
        public DateTime? FechaUltimoServicio { get; set; }
        public DateTime VencimientoSoat { get; set; }
        public DateTime VencimientoTecnomecanica { get; set; }
        public DateTime VencimientoLicencia { get; set; }
    }
}