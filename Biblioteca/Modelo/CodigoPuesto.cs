using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // CAJUELA, PASILLO, VENTANA. SIRVE PARA VALIDACIONES POSTERIORES
    public class CodigoPuesto
    {
        public long Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Puesto> Puestos { get; set; } = new HashSet<Puesto>();
    }
}
