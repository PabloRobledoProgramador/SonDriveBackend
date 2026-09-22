using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Pasajero
    {
        public long Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string? Direccion { get; set; }

        public ICollection<Ocupacion> Ocupaciones { get; set; } = new List<Ocupacion>();
    }
}
