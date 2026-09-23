using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Vehiculo para crear viajes, Monitoreo y page de Conductores
    public class Vehiculo
    {
        public long Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string AnnoModelo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;

        // Relaciones
        public long IdUsuario { get; set; }
        public Usuario Usuario { get; set; } = null!;

        // Propiedades 
        public ICollection<Puesto> Puestos { get; set; } = new HashSet<Puesto>();
        public ICollection<Viaje> Viajes { get; set; } = new HashSet<Viaje>();
        public ICollection<Falla> Fallas { get; set; } = new HashSet<Falla>();
    }
}
