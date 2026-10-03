using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Falla
{
    public class ActualizarFallaDTO
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Severidad { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
