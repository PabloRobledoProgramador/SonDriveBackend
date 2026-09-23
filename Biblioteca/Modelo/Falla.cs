using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Modelo
{
    // Para la page de Vehiculos
    public class Falla
    {
        public long Id { get; set; }

        // Relacion N:1
        public long VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        // Datos 
        public string Descripcion { get; set; } = string.Empty;
        public string Severidad { get; set; } = string.Empty; // "Alerta", "Crítico", etc.
        public DateTime Fecha { get; set; }
        public bool EstaResuelta { get; set; } = false;
        
    }
}
