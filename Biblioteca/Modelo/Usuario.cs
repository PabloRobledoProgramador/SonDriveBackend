using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Administradores o conductores de la aplicación
    public class Usuario
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string Rol { get; set; } = "Conductor"; // Conductor por defecto
        public string? Foto { get; set; }

        public ICollection<Vehiculo> Vehiculos { get; set; } = new HashSet<Vehiculo>();
    }
}
