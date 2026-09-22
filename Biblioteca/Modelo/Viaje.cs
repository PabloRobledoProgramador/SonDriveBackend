using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Viaje
    {
        public long Id { get; set; }
        public DateTime FechaSalida { get; set; }
        public long Origen { get; set; }
        public long Destino { get; set; }
        public double Precio { get; set; }
        public int IdHora { get; set; }
        public long IdVehiculo { get; set; }

        // Relaciones
        public Ciudad CiudadOrigen { get; set; } = null!;
        public Ciudad CiudadDestino { get; set; } = null!;
        public Hora Hora { get; set; } = null!;
        public Vehiculo Vehiculo { get; set; } = null!;

        public ICollection<Ocupacion> Ocupaciones { get; set; } = new List<Ocupacion>();
    }
}
