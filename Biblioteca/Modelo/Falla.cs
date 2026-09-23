using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    public class Falla
    {
        public int Id { get; set; }

        // Relacion N:1
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        // Datos 
        public string Descripcion { get; set; } = string.Empty;
        public string Severidad { get; set; } = string.Empty; // "Alerta", "Crítico", etc.
        public DateTime Fecha { get; set; }
        public bool EstaResuelta { get; set; } = false;
        
    }
}
