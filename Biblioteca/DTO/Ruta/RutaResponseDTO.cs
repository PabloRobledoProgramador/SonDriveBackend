using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Ruta
{
    public class RutaResponseDTO
    {
        public long Id { get; set; }

        public long CiudadOrigenId { get; set; }
        public string CiudadOrigen { get; set; } = string.Empty;

        public long CiudadDestinoId { get; set; }
        public string CiudadDestino { get; set; } = string.Empty;

        public decimal PrecioBase { get; set; }
        public double DistanciaKm { get; set; }
        public string TiempoEstimado { get; set; } = string.Empty;
    }
}
