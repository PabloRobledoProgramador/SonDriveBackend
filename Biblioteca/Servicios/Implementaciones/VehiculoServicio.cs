using AutoMapper;
using Biblioteca.DTO.Vehiculo;
using Biblioteca.Excepciones;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Seguridad;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class VehiculoServicio : IVehiculoServicio
    {
        private const string Critico = "Crítico";
        private const string Alerta = "Alerta";
        private const string Optimo = "Óptimo";

        private readonly IVehiculoRepositorio _repositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IMapper _mapper;
        private readonly IPuestoServicio _puestoServicio;

        public VehiculoServicio(
            IVehiculoRepositorio repositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IMapper mapper,
            IPuestoServicio puestoServicio)
        {
            _repositorio = repositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
            _puestoServicio = puestoServicio;
        }

        public async Task<IEnumerable<VehiculoResponseDTO>> ListarAsync(CancellationToken ct = default)
        {
            var vehiculos = await _repositorio.ListarAsync(ct);
            return vehiculos.Select(Mapear);
        }

        public async Task<IEnumerable<VehiculoResponseDTO>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default)
        {
            var vehiculos = await _repositorio.ListarPorUsuarioAsync(idUsuario, ct);
            return vehiculos.Select(Mapear);
        }

        public async Task<VehiculoResponseDTO> ObtenerPorIdAsync(long id, CancellationToken ct = default)
        {
            var vehiculo = await ObtenerOFallarAsync(id, true, ct);
            return Mapear(vehiculo);
        }

        public async Task<VehiculoResponseDTO> CrearAsync(VehiculoDatosDTO dto, CancellationToken ct = default)
        {
            await ValidarConductorAsync(dto.IdUsuario, ct);

            var placa = NormalizarPlaca(dto.Placa);
            if (await _repositorio.ExistePlacaAsync(placa, null, ct))
                throw new ReglaNegocioException($"Ya existe un vehículo con la placa '{placa}'.");

            var vehiculo = _mapper.Map<Vehiculo>(dto);
            vehiculo.Placa = placa;

            var creado = await _repositorio.CrearAsync(vehiculo, ct);

            // El vehículo es el dueño de sus puestos: se genera el mapa apenas se crea.
            await _puestoServicio.AsegurarMapaAsync(creado.Id, ct);

            return Mapear(await ObtenerOFallarAsync(creado.Id, true, ct));
        }

        public async Task<VehiculoResponseDTO> ActualizarAsync(long id, VehiculoDatosDTO dto, CancellationToken ct = default)
        {
            var existente = await ObtenerOFallarAsync(id, false, ct);

            await ValidarConductorAsync(dto.IdUsuario, ct);

            var placa = NormalizarPlaca(dto.Placa);
            if (await _repositorio.ExistePlacaAsync(placa, id, ct))
                throw new ReglaNegocioException($"Ya existe otro vehículo con la placa '{placa}'.");

            _mapper.Map(dto, existente);
            existente.Placa = placa;

            await _repositorio.ActualizarAsync(existente, ct);
            return Mapear(await ObtenerOFallarAsync(id, true, ct));
        }

        public async Task EliminarAsync(long id, CancellationToken ct = default)
        {
            await ObtenerOFallarAsync(id, false, ct);

            if (await _repositorio.TieneViajesAsync(id, ct))
                throw new ReglaNegocioException("No se puede eliminar el vehículo porque tiene viajes registrados.");

            await _repositorio.EliminarAsync(id, ct);
        }

        private async Task<Vehiculo> ObtenerOFallarAsync(long id, bool incluirDetalle, CancellationToken ct)
        {
            return await _repositorio.ObtenerPorIdAsync(id, incluirDetalle, ct)
                ?? throw new NoEncontradoException($"No existe el vehículo con id {id}.");
        }

        private async Task ValidarConductorAsync(long idUsuario, CancellationToken ct)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(idUsuario, false, ct)
                ?? throw new NoEncontradoException($"No existe el usuario con id {idUsuario}.");

            if (usuario.Rol != Roles.Conductor)
                throw new ReglaNegocioException("El usuario seleccionado no tiene rol de conductor.");

            if (!usuario.Estado)
                throw new ReglaNegocioException("El conductor seleccionado está inactivo.");
        }

        private static string NormalizarPlaca(string placa)
        {
            return placa.Replace(" ", string.Empty).ToUpperInvariant();
        }

        private static VehiculoResponseDTO Mapear(Vehiculo v)
        {
            var hoy = DateTime.Today;
            var pendientes = v.Fallas.Where(f => !f.EstaResuelta).ToList();

            var estado = pendientes.Any(f => f.Severidad == Critico) ? Critico
                : pendientes.Any(f => f.Severidad == Alerta) ? Alerta
                : Optimo;

            return new VehiculoResponseDTO
            {
                Id = v.Id,
                NombreVehiculo = $"{v.Modelo} {v.AnnoModelo}",
                Placa = v.Placa,
                Modelo = v.Modelo,
                AnnoModelo = v.AnnoModelo,
                Tipo = v.Tipo,
                IdUsuario = v.IdUsuario,
                Conductor = $"{v.Usuario.Nombre} {v.Usuario.Apellido}",
                FotoConductor = v.Usuario.Foto,
                Kilometraje = v.Kilometraje,
                FechaUltimoServicio = v.FechaUltimoServicio,
                Estado = estado,
                FallasPendientes = pendientes.Count,
                Soat = Documento(v.VencimientoSoat, hoy),
                Tecnomecanica = Documento(v.VencimientoTecnomecanica, hoy),
                Licencia = Documento(v.VencimientoLicencia, hoy)
            };
        }

        private static DocumentoVencimientoDTO Documento(DateTime vencimiento, DateTime hoy)
        {
            return new DocumentoVencimientoDTO
            {
                Vencimiento = vencimiento,
                DiasRestantes = (vencimiento.Date - hoy).Days
            };
        }
    }
}