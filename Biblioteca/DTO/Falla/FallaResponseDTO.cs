using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Falla
{
    public class FallaResponseDTO
    {
        public long Id { get; set; }
        public long VehiculoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Severidad { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool EstaResuelta { get; set; }
    }
}
