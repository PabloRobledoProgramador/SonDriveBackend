using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Ciudad
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Propiedades
        public ICollection<Viaje> ViajesOrigen { get; set; } = new List<Viaje>();
        public ICollection<Viaje> ViajesDestino { get; set; } = new List<Viaje>();
    }
}
