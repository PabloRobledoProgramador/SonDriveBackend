using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Para el Navegador y para Crear Viajes
    public class Hora
    {
        public long Id { get; set; }
        public string ValorHora { get; set; } = string.Empty; 

        // Propiedades
        public ICollection<Viaje> Viajes { get; set; } = new HashSet<Viaje>();
    }
}
