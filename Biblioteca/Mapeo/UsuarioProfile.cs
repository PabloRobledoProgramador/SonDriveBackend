using AutoMapper;
using Biblioteca.DTO.Usuario;
using Biblioteca.Modelo;

namespace Biblioteca.Mapeo
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            // Entidad -> respuesta
            CreateMap<Usuario, UsuarioDto>();

            // Crear -> entidad nueva
            CreateMap<UsuarioCrearDto, Usuario>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Vehiculos, o => o.Ignore())
                .ForMember(d => d.Rol, o => o.Condition(s => !string.IsNullOrWhiteSpace(s.Rol)));

            // Actualizar -> se mapea sobre la entidad ya existente
            CreateMap<UsuarioActualizarDto, Usuario>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Clave, o => o.Ignore())
                .ForMember(d => d.Vehiculos, o => o.Ignore())
                .ForMember(d => d.Rol, o => o.Condition(s => !string.IsNullOrWhiteSpace(s.Rol)));

            // Datos del conductor -> entidad existente: nunca toca clave, estado ni rol
            CreateMap<ConductorActualizarDto, Usuario>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Clave, o => o.Ignore())
                .ForMember(d => d.Estado, o => o.Ignore())
                .ForMember(d => d.Rol, o => o.Ignore())
                .ForMember(d => d.Vehiculos, o => o.Ignore());
        }
    }
}