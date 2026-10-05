using Biblioteca.DTO.Ocupacion;
using Biblioteca.Excepciones;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Servicios.Implementaciones
{
    public class OcupacionServicio : IOcupacionServicio
    {
        private readonly IOcupacionRepositorio _repositorio;
        private readonly IViajeRepositorio _viajeRepositorio;

        public OcupacionServicio(IOcupacionRepositorio repositorio, IViajeRepositorio viajeRepositorio)
        {
            _repositorio = repositorio;
            _viajeRepositorio = viajeRepositorio;
        }

        public async Task<IEnumerable<OcupacionResponseDTO>> ListarPorViajeAsync(long idViaje, CancellationToken ct = default)
        {
            await ObtenerViajeOFallarAsync(idViaje, ct);
            var ocupaciones = await _repositorio.ListarPorViajeAsync(idViaje, ct);
            return ocupaciones.Select(AResponse).ToList();
        }

        public async Task<OcupacionResponseDTO> OcuparAsync(long idViaje, long idPuesto, CrearOcupacionDTO dto, CancellationToken ct = default)
        {
            var viaje = await ObtenerViajeOFallarAsync(idViaje, ct);

            if (viaje.FechaSalida <= DateTime.Now)
                throw new ReglaNegocioException("El viaje ya salió; no se pueden modificar sus puestos.");

            var puesto = viaje.Vehiculo.Puestos.FirstOrDefault(p => p.Id == idPuesto)
                ?? throw new NoEncontradoException($"El puesto {idPuesto} no pertenece al vehículo de este viaje.");

            if (MapaPuestos.EsConductor(puesto))
                throw new ReglaNegocioException("El puesto del conductor no se puede modificar.");

            if (viaje.Ocupaciones.Any(o => o.IdPuesto == idPuesto))
                throw new ReglaNegocioException("Ese puesto ya está ocupado en este viaje.");

            var ocupacion = new Ocupacion
            {
                IdViaje = idViaje,
                IdPuesto = idPuesto,
                Ocupado = true,
                Pasajero = new Pasajero
                {
                    NombreCompleto = dto.NombreCompleto.Trim(),
                    Celular = dto.Celular.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim()
                }
            };

            try
            {
                await _repositorio.CrearAsync(ocupacion, ct);
            }
            catch (DbUpdateException)
            {
                // Índice único (IdViaje, IdPuesto): alguien lo ocupó justo antes.
                throw new ReglaNegocioException("Ese puesto ya está ocupado en este viaje.");
            }

            var creada = await _repositorio.ObtenerAsync(idViaje, idPuesto, ct)
                ?? throw new NoEncontradoException("No se pudo leer la ocupación creada.");
            return AResponse(creada);
        }

        public async Task LiberarAsync(long idViaje, long idPuesto, CancellationToken ct = default)
        {
            await ObtenerViajeOFallarAsync(idViaje, ct);

            var ocupacion = await _repositorio.ObtenerAsync(idViaje, idPuesto, ct)
                ?? throw new NoEncontradoException("Ese puesto no está ocupado en este viaje.");

            await _repositorio.EliminarAsync(ocupacion.Id, ct);
        }

        private async Task<Viaje> ObtenerViajeOFallarAsync(long idViaje, CancellationToken ct)
        {
            return await _viajeRepositorio.ObtenerPorIdAsync(idViaje, true, ct)
                ?? throw new NoEncontradoException($"No existe el viaje con id {idViaje}.");
        }

        private static OcupacionResponseDTO AResponse(Ocupacion o)
        {
            return new OcupacionResponseDTO
            {
                Id = o.Id,
                IdViaje = o.IdViaje,
                IdPuesto = o.IdPuesto,
                Codigo = o.Puesto.CodigoPuesto.Descripcion,
                Tipo = o.Puesto.TipoPuesto.Descripcion,
                Pasajero = new PasajeroOcupacionDTO
                {
                    NombreCompleto = o.Pasajero.NombreCompleto,
                    Celular = o.Pasajero.Celular,
                    Direccion = o.Pasajero.Direccion
                }
            };
        }
    }
}