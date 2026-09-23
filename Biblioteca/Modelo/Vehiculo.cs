using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Vehiculo
    {
        public long Id { get; set; }
        public long IdUsuario { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string AnnoModelo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public ICollection<Puesto> Puestos { get; set; } = new List<Puesto>();
        public ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
        public ICollection<Falla> Fallas { get; set; } = new List<Falla>();
    }
}
