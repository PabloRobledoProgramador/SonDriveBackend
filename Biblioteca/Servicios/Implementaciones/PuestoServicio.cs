using Biblioteca.Excepciones;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class PuestoServicio : IPuestoServicio
    {
        private readonly IPuestoRepositorio _repositorio;
        private readonly IVehiculoRepositorio _vehiculoRepositorio;

        public PuestoServicio(IPuestoRepositorio repositorio, IVehiculoRepositorio vehiculoRepositorio)
        {
            _repositorio = repositorio;
            _vehiculoRepositorio = vehiculoRepositorio;
        }

        public async Task AsegurarMapaAsync(long idVehiculo, CancellationToken ct = default)
        {
            if (await _vehiculoRepositorio.ObtenerPorIdAsync(idVehiculo, false, ct) is null)
                throw new NoEncontradoException($"No existe el vehículo con id {idVehiculo}.");

            var existentes = await _repositorio.ListarPorVehiculoAsync(idVehiculo, ct);
            var codigosExistentes = existentes
                .Select(p => p.CodigoPuesto.Descripcion.ToLowerInvariant())
                .ToHashSet();

            var nuevos = new List<Puesto>();
            foreach (var (codigo, tipo) in MapaPuestos.Estandar)
            {
                if (codigosExistentes.Contains(codigo.ToLowerInvariant()))
                    continue;

                var cod = await _repositorio.ObtenerOCrearCodigoAsync(codigo, ct);
                var tip = await _repositorio.ObtenerOCrearTipoAsync(tipo, ct);

                nuevos.Add(new Puesto { IdVehiculo = idVehiculo, IdCodigo = cod.Id, IdTipo = tip.Id });
            }

            if (nuevos.Count > 0)
                await _repositorio.AgregarRangoAsync(nuevos, ct);
        }
    }
}