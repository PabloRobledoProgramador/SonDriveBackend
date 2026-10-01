using Biblioteca.Modelo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Repositorios.Interfaces
{
    public interface ICiudadRepositorio
    {
        Task<IReadOnlyList<Ciudad>> ListarAsync(CancellationToken ct = default);
    }
}
