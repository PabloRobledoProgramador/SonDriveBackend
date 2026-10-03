
using AutoMapper;
using Biblioteca.DTO.Falla;
using Biblioteca.Excepciones;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;

namespace Biblioteca.Servicios.Implementaciones
{
    public class FallaServicio : IFallaServicio
    {
        private readonly IFallaRepositorio _repositorio;
        private readonly IVehiculoRepositorio _vehiculoRepositorio;
        private readonly IMapper _mapper;

        public FallaServicio(
            IFallaRepositorio repositorio,
            IVehiculoRepositorio vehiculoRepositorio,
            IMapper mapper)
        {
            _repositorio = repositorio;
            _vehiculoRepositorio = vehiculoRepositorio;
            _mapper = mapper;
        }

        public async Task<IEnumerable<FallaResponseDTO>> ListarPorVehiculoAsync(long idVehiculo, CancellationToken ct = default)
        {
            await ValidarVehiculoAsync(idVehiculo, ct);

            var fallas = await _repositorio.ListarPorVehiculoAsync(idVehiculo, ct);
            return _mapper.Map<IEnumerable<FallaResponseDTO>>(fallas);
        }

        public async Task<FallaResponseDTO> ReportarAsync(ReportarFallaDTO dto, CancellationToken ct = default)
        {
            await ValidarVehiculoAsync(dto.VehiculoId, ct);

            var falla = _mapper.Map<Falla>(dto);

            var creada = await _repositorio.CrearAsync(falla, ct);
            return _mapper.Map<FallaResponseDTO>(creada);
        }

        public async Task<FallaResponseDTO> ActualizarAsync(long id, ActualizarFallaDTO dto, CancellationToken ct = default)
        {
            var falla = await ObtenerOFallarAsync(id, ct);

            if (falla.EstaResuelta)
                throw new ReglaNegocioException("No se puede editar una falla resuelta. Reábrela primero.");

            _mapper.Map(dto, falla);

            var actualizada = await _repositorio.ActualizarAsync(falla, ct);
            return _mapper.Map<FallaResponseDTO>(actualizada);
        }

        public async Task<FallaResponseDTO> CambiarResueltaAsync(long id, bool resuelta, CancellationToken ct = default)
        {
            var falla = await ObtenerOFallarAsync(id, ct);

            falla.EstaResuelta = resuelta;

            var actualizada = await _repositorio.ActualizarAsync(falla, ct);
            return _mapper.Map<FallaResponseDTO>(actualizada);
        }

        public async Task EliminarAsync(long id, CancellationToken ct = default)
        {
            if (!await _repositorio.EliminarAsync(id, ct))
                throw new NoEncontradoException($"No existe la falla con id {id}.");
        }

        private async Task<Falla> ObtenerOFallarAsync(long id, CancellationToken ct)
        {
            return await _repositorio.ObtenerPorIdAsync(id, ct)
                ?? throw new NoEncontradoException($"No existe la falla con id {id}.");
        }

        private async Task ValidarVehiculoAsync(long idVehiculo, CancellationToken ct)
        {
            if (await _vehiculoRepositorio.ObtenerPorIdAsync(idVehiculo, false, ct) is null)
                throw new NoEncontradoException($"No existe el vehículo con id {idVehiculo}.");
        }
    }
}