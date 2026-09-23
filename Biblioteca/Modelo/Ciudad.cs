using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Catalogo de Ciudades para Rutas y Buscador de Viajes
    public class Ciudad
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        // Propiedades
        public ICollection<Viaje> ViajesOrigen { get; set; } = new HashSet<Viaje>();
        public ICollection<Viaje> ViajesDestino { get; set; } = new HashSet<Viaje>();
        public ICollection<Ruta> RutasComoOrigen { get; set; } = new HashSet<Ruta>();
        public ICollection<Ruta> RutasComoDestino { get; set; } = new HashSet<Ruta>();
    }
}
