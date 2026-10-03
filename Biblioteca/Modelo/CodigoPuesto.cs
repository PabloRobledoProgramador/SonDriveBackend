using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    
    // A1, A2, COPILOTO, CONDUCTOR
    public class CodigoPuesto
    {
        public long Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Puesto> Puestos { get; set; } = new HashSet<Puesto>();
    }
}
