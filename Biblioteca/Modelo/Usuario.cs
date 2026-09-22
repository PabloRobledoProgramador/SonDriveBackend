using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Usuario
    {
        public long Id { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? Foto { get; set; }

        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }
}
