using AutoMapper;
using Biblioteca.DTO.Falla;
using Biblioteca.Modelo;

namespace Biblioteca.Mapeo
{
    public class FallaProfile : Profile
    {
        public FallaProfile()
        {
            CreateMap<Falla, FallaResponseDTO>();

            CreateMap<ReportarFallaDTO, Falla>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Vehiculo, o => o.Ignore())
                .ForMember(d => d.EstaResuelta, o => o.Ignore())
                .ForMember(d => d.Descripcion, o => o.MapFrom(s => s.Descripcion.Trim()))
                .ForMember(d => d.Fecha, o => o.MapFrom(s => s.Fecha.Date));

            CreateMap<ActualizarFallaDTO, Falla>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.VehiculoId, o => o.Ignore())
                .ForMember(d => d.Vehiculo, o => o.Ignore())
                .ForMember(d => d.EstaResuelta, o => o.Ignore())
                .ForMember(d => d.Descripcion, o => o.MapFrom(s => s.Descripcion.Trim()))
                .ForMember(d => d.Fecha, o => o.MapFrom(s => s.Fecha.Date));
        }
    }
}