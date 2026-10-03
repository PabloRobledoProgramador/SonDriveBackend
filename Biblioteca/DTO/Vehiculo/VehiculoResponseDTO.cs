namespace Biblioteca.DTO.Vehiculo
{
    public class VehiculoResponseDTO
    {
        public long Id { get; set; }
        public string NombreVehiculo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string AnnoModelo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public long IdUsuario { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public string? FotoConductor { get; set; }
        public int Kilometraje { get; set; }
        public DateTime? FechaUltimoServicio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int FallasPendientes { get; set; }
        public DocumentoVencimientoDTO Soat { get; set; } = new();
        public DocumentoVencimientoDTO Tecnomecanica { get; set; } = new();
        public DocumentoVencimientoDTO Licencia { get; set; } = new();
    }
}