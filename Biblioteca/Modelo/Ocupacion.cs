using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Ocupacion
    {
        public long Id { get; set; }
        public long IdViaje { get; set; }
        public long IdPuesto { get; set; }
        public long IdPasajero { get; set; }
        public bool Estado { get; set; }

        // Relaciones
        public Viaje Viaje { get; set; } = null!;
        public Puesto Puesto { get; set; } = null!;
        public Pasajero Pasajero { get; set; } = null!;
    }
}
