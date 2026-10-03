using AutoMapper;
using Biblioteca.DTO.Vehiculo;
using Biblioteca.Modelo;

namespace Biblioteca.Mapeo
{
    public class VehiculoProfile : Profile
    {
        public VehiculoProfile()
        {
            CreateMap<VehiculoDatosDTO, Vehiculo>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Placa, o => o.Ignore())
                .ForMember(d => d.Usuario, o => o.Ignore())
                .ForMember(d => d.Puestos, o => o.Ignore())
                .ForMember(d => d.Viajes, o => o.Ignore())
                .ForMember(d => d.Fallas, o => o.Ignore())
                .ForMember(d => d.Modelo, o => o.MapFrom(s => s.Modelo.Trim()))
                .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.Trim()))
                .ForMember(d => d.FechaUltimoServicio, o => o.MapFrom(s => s.FechaUltimoServicio.HasValue ? s.FechaUltimoServicio.Value.Date : (DateTime?)null))
                .ForMember(d => d.VencimientoSoat, o => o.MapFrom(s => s.VencimientoSoat.Date))
                .ForMember(d => d.VencimientoTecnomecanica, o => o.MapFrom(s => s.VencimientoTecnomecanica.Date))
                .ForMember(d => d.VencimientoLicencia, o => o.MapFrom(s => s.VencimientoLicencia.Date));
        }
    }
}