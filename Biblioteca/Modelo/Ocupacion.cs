using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Representa un ticket o reserva de un pasajero cuando el conductor ocupa un puesto
    public class Ocupacion
    {
        public long Id { get; set; }
        public bool Ocupado { get; set; } = true;

        // Relaciones N:1
        public long IdViaje { get; set; }
        public Viaje Viaje { get; set; } = null!;

        public long IdPuesto { get; set; }
        public Puesto Puesto { get; set; } = null!;

        public long IdPasajero { get; set; }
        public Pasajero Pasajero { get; set; } = null!;
    }
}
