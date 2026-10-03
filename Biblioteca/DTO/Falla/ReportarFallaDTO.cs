using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Falla
{
    // Usuario registra en el Servidor
    public class ReportarFallaDTO
    {
        public long VehiculoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Severidad { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
