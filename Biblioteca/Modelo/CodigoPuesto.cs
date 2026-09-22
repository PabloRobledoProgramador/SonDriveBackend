using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class CodigoPuesto
    {
        public long Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Puesto> Puestos { get; set; } = new List<Puesto>();
    }
}
