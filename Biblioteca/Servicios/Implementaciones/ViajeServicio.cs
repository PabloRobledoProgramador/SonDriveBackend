using Biblioteca.DTO.Viaje;
using Biblioteca.Excepciones;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class ViajeServicio : IViajeServicio
    {
        private readonly IViajeRepositorio _repositorio;
        private readonly IPuestoServicio _puestoServicio;

        public ViajeServicio(IViajeRepositorio repositorio, IPuestoServicio puestoServicio)
        {
            _repositorio = repositorio;
            _puestoServicio = puestoServicio;
        }

        public async Task<IEnumerable<ViajeResumenDTO>> BuscarAsync(BuscarViajeDTO filtro, CancellationToken ct = default)
        {
            var viajes = await _repositorio.BuscarAsync(
                filtro.CiudadOrigenId, filtro.CiudadDestinoId, filtro.Fecha, filtro.Hora, ct);

            return viajes.Select(AResumen).ToList();
        }

        public async Task<IEnumerable<ViajeResumenDTO>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default)
        {
            var viajes = await _repositorio.ListarPorUsuarioAsync(idUsuario, ct);
            return viajes.Select(AResumen).ToList();
        }

        public async Task<ViajeResponseDTO> ObtenerPorIdAsync(long id, CancellationToken ct = default)
        {
            var viaje = await ObtenerOFallarAsync(id, true, ct);
            return AResponse(viaje);
        }

        public async Task<ViajeResumenDTO> CrearAsync(CrearViajeDTO dto, CancellationToken ct = default)
        {
            // El viaje NO crea puestos: solo se asegura de que el vehículo tenga su mapa (y de que exista).
            await _puestoServicio.AsegurarMapaAsync(dto.IdVehiculo, ct);

            var fechaSalida = CombinarFechaHora(dto);
            await ValidarVehiculoLibreAsync(dto.IdVehiculo, fechaSalida, null, ct);

            var viaje = new Viaje
            {
                IdVehiculo = dto.IdVehiculo,
                CiudadOrigenId = dto.CiudadOrigenId,
                CiudadDestinoId = dto.CiudadDestinoId,
                FechaSalida = fechaSalida,
                Precio = dto.Precio
            };

            var creado = await _repositorio.CrearAsync(viaje, ct);
            return AResumen(await ObtenerOFallarAsync(creado.Id, true, ct));
        }

        public async Task<ViajeResumenDTO> ActualizarAsync(long id, CrearViajeDTO dto, CancellationToken ct = default)
        {
            var existente = await ObtenerOFallarAsync(id, false, ct);

            if (existente.IdVehiculo != dto.IdVehiculo && await _repositorio.TieneOcupacionesAsync(id, ct))
                throw new ReglaNegocioException("No se puede cambiar el vehículo porque el viaje ya tiene puestos ocupados.");

            await _puestoServicio.AsegurarMapaAsync(dto.IdVehiculo, ct);

            var fechaSalida = CombinarFechaHora(dto);
            await ValidarVehiculoLibreAsync(dto.IdVehiculo, fechaSalida, id, ct);

            existente.IdVehiculo = dto.IdVehiculo;
            existente.CiudadOrigenId = dto.CiudadOrigenId;
            existente.CiudadDestinoId = dto.CiudadDestinoId;
            existente.FechaSalida = fechaSalida;
            existente.Precio = dto.Precio;

            await _repositorio.ActualizarAsync(existente, ct);
            return AResumen(await ObtenerOFallarAsync(id, true, ct));
        }

        public async Task EliminarAsync(long id, CancellationToken ct = default)
        {
            await ObtenerOFallarAsync(id, false, ct);
            await _repositorio.EliminarAsync(id, ct);
        }

        private async Task<Viaje> ObtenerOFallarAsync(long id, bool incluirDetalle, CancellationToken ct)
        {
            return await _repositorio.ObtenerPorIdAsync(id, incluirDetalle, ct)
                ?? throw new NoEncontradoException($"No existe el viaje con id {id}.");
        }

        private async Task ValidarVehiculoLibreAsync(long idVehiculo, DateTime fechaSalida, long? excluirId, CancellationToken ct)
        {
            if (await _repositorio.ExisteViajeVehiculoAsync(idVehiculo, fechaSalida, excluirId, ct))
                throw new ReglaNegocioException("El vehículo ya tiene un viaje programado a esa fecha y hora.");
        }

        private static DateTime CombinarFechaHora(CrearViajeDTO dto)
        {
            var salida = dto.Fecha.Date + dto.Hora;
            return new DateTime(salida.Year, salida.Month, salida.Day, salida.Hour, salida.Minute, 0);
        }

        private static HashSet<long> PuestosOcupados(Viaje v)
        {
            return v.Ocupaciones.Where(o => o.Ocupado).Select(o => o.IdPuesto).ToHashSet();
        }

        private static ViajeResumenDTO AResumen(Viaje v) => Construir<ViajeResumenDTO>(v);

        private static ViajeResponseDTO AResponse(Viaje v)
        {
            var ocupados = PuestosOcupados(v);
            var response = Construir<ViajeResponseDTO>(v);

            response.NombreVehiculo = $"{v.Vehiculo.Modelo} {v.Vehiculo.AnnoModelo}";
            response.Placa = v.Vehiculo.Placa;
            response.TelefonoConductor = v.Vehiculo.Usuario.Celular;
            response.Puestos = v.Vehiculo.Puestos
                .OrderBy(p => p.Id)
                .Select(p => new PuestoViajeDTO
                {
                    IdPuesto = p.Id,
                    Codigo = p.CodigoPuesto.Descripcion,
                    Tipo = p.TipoPuesto.Descripcion,
                    EsConductor = MapaPuestos.EsConductor(p),
                    Ocupado = MapaPuestos.EsConductor(p) || ocupados.Contains(p.Id)
                })
                .ToList();

            return response;
        }

        private static T Construir<T>(Viaje v) where T : ViajeResumenDTO, new()
        {
            var ocupados = PuestosOcupados(v);
            var puestos = v.Vehiculo.Puestos;

            return new T
            {
                Id = v.Id,
                FechaSalida = v.FechaSalida,
                Precio = v.Precio,
                CiudadOrigenId = v.CiudadOrigenId,
                CiudadOrigen = v.CiudadOrigen.Nombre,
                CiudadDestinoId = v.CiudadDestinoId,
                CiudadDestino = v.CiudadDestino.Nombre,
                IdVehiculo = v.IdVehiculo,
                Conductor = $"{v.Vehiculo.Usuario.Nombre} {v.Vehiculo.Usuario.Apellido}",
                FotoConductor = v.Vehiculo.Usuario.Foto,
                CuposLibres = puestos.Count(p => !MapaPuestos.EsConductor(p) && !MapaPuestos.EsCajuela(p) && !ocupados.Contains(p.Id)),
                CajuelasLibres = puestos.Count(p => MapaPuestos.EsCajuela(p) && !ocupados.Contains(p.Id))
            };
        }
    }
}