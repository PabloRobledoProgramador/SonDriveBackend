using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Puesto
    {
        public long Id { get; set; }
        public long IdVehiculo { get; set; }
        public long IdCodigo { get; set; }
        public long IdTipo { get; set; }

        // Relaciones
        public Vehiculo Vehiculo { get; set; } = null!;
        public CodigoPuesto CodigoPuesto { get; set; } = null!;
        public TipoPuesto TipoPuesto { get; set; } = null!;

        public ICollection<Ocupacion> Ocupaciones { get; set; } = new List<Ocupacion>();
    }
}
