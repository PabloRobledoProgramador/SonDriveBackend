using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO
{
    // Para la página de conductores
    public class VehiculoResumenDTO
    {
        public long Id { get; set; }
        public string NombreVehiculo { get; set; } = string.Empty; 
        public string Placa { get; set; } = string.Empty;        
    }
}
