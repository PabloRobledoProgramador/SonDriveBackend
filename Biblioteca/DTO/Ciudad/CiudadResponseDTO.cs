using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Ciudad
{
    // Para el desplegable de Crear Viaje y Buscar viaje
    public class CiudadResponseDTO
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
