using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Hora
    {
        public int Id { get; set; }
        public string ValorHora { get; set; } = string.Empty; 

        public ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();
    }
}
