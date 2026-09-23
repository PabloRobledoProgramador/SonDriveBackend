using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Puesto
    {
        public long Id { get; set; }
        

        // Relaciones N:1
        public long IdVehiculo { get; set; }
        public Vehiculo Vehiculo { get; set; } = null!;

        public long IdCodigo { get; set; }
        public CodigoPuesto CodigoPuesto { get; set; } = null!;
        
        public long IdTipo { get; set; }
        public TipoPuesto TipoPuesto { get; set; } = null!;

        // Propiedades
        public ICollection<Ocupacion> Ocupaciones { get; set; } = new List<Ocupacion>();
    }
}
