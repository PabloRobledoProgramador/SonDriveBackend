using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Representa un Viaje tanto en la Page principal como en Crear Viajes
    public class Viaje
    {
        public long Id { get; set; }
        public DateTime FechaSalida { get; set; }
        public decimal Precio { get; set; }

        // Relaciones
        public long CiudadOrigenId { get; set; }
        public Ciudad CiudadOrigen { get; set; } = null!;

        public long CiudadDestinoId { get; set; }
        public Ciudad CiudadDestino { get; set; } = null!;

        public long IdHora { get; set; }
        public Hora Hora { get; set; } = null!;

        public long IdVehiculo { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        // Propiedades
        public ICollection<Ocupacion> Ocupaciones { get; set; } = new HashSet<Ocupacion>();
    }
}
