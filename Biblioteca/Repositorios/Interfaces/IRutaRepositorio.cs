using Biblioteca.Modelo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface IRutaRepositorio
    {
        Task<IReadOnlyList<Ruta>> ListarAsync(CancellationToken ct = default);
    }
}
