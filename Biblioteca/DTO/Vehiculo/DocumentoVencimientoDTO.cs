using System;
namespace Biblioteca.DTO.Vehiculo
{
    public class DocumentoVencimientoDTO
    {
        public DateTime Vencimiento { get; set; }
        public int DiasRestantes { get; set; }
    }
}