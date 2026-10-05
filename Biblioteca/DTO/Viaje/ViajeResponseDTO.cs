namespace Biblioteca.DTO.Viaje
{
    public class ViajeResponseDTO : ViajeResumenDTO
    {
        public string NombreVehiculo { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string TelefonoConductor { get; set; } = string.Empty;
        public List<PuestoViajeDTO> Puestos { get; set; } = new();
    }
}