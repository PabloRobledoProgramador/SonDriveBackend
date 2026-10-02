using Biblioteca.DTO.Ruta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Servicios.Interfaces
{
    public interface IRutaServicio 
    {
        Task<IEnumerable<RutaResponseDTO>> ListarAsync(CancellationToken ct = default);
    }
}
