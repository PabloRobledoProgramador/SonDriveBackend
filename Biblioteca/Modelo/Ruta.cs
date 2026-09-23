using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Ruta
    {
        
        public long Id { get; set; }

        // Relacion N:1
        public long CiudadOrigenId { get; set; }
        public Ciudad? CiudadOrigen { get; set; }

        public long CiudadDestinoId { get; set; }
        public Ciudad? CiudadDestino { get; set; }

        // Datos 
        public decimal PrecioBase { get; set; }
        public double DistanciaKm { get; set; }
        public string TiempoEstimado { get; set; } = string.Empty;
        
    }
}
