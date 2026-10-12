using AutoMapper;
using Biblioteca.DTO.Usuario;
using Biblioteca.Excepciones;
using Biblioteca.Mapeo;
using Biblioteca.Modelo;
using Biblioteca.Pruebas.Infraestructura;
using Biblioteca.Repositorios.Implementaciones;
using Biblioteca.Seguridad;
using Biblioteca.Servicios.Implementaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Biblioteca.Pruebas.Servicios;

public sealed class UsuarioServicioPruebas : IDisposable
{
    private const string Clave = "Clave12345*";

    private readonly BaseDatosEnMemoria _baseDatos = new();

    private UsuarioServicio CrearServicio()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<UsuarioProfile>(), NullLoggerFactory.Instance)
            .CreateMapper();
        return new UsuarioServicio(new UsuarioRepositorio(_baseDatos.CrearConexion()), mapper);
    }

    private static ConductorActualizarDto DatosValidos(string correo = "nuevo@sondrive.com") => new()
    {
        Nombre = "Carla",
        Apellido = "Gómez",
        Correo = correo,
        Celular = "3109876543",
        Foto = "https://ejemplo.com/carla.png"
    };

    private async Task<Usuario> LeerUsuarioAsync(long id)
    {
        await using var contexto = _baseDatos.CrearConexion();
        return await contexto.Usuarios.AsNoTracking().SingleAsync(u => u.Id == id);
    }

    [Fact]
    public async Task ActualizarConductorAsync_DatosValidos_GuardaLosCincoDatos()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: true);

        var respuesta = await CrearServicio().ActualizarConductorAsync(conductor.Id, DatosValidos());

        Assert.Equal(conductor.Id, respuesta.Id);
        Assert.Equal("Carla", respuesta.Nombre);
        var guardado = await LeerUsuarioAsync(conductor.Id);
        Assert.Equal("Carla", guardado.Nombre);
        Assert.Equal("Gómez", guardado.Apellido);
        Assert.Equal("nuevo@sondrive.com", guardado.Correo);
        Assert.Equal("3109876543", guardado.Celular);
        Assert.Equal("https://ejemplo.com/carla.png", guardado.Foto);
    }

    [Fact]
    public async Task ActualizarConductorAsync_UsuarioInexistente_LanzaNoEncontradoException()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => CrearServicio().ActualizarConductorAsync(9999, DatosValidos()));
    }

    [Fact]
    public async Task ActualizarConductorAsync_CorreoDeOtroUsuario_LanzaReglaNegocioException()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: true);
        await _baseDatos.AgregarUsuarioAsync("laura@sondrive.com", Clave, Roles.Conductor, estado: true);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ActualizarConductorAsync(conductor.Id, DatosValidos("laura@sondrive.com")));

        Assert.Equal("carlos@sondrive.com", (await LeerUsuarioAsync(conductor.Id)).Correo);
    }

    [Fact]
    public async Task ActualizarConductorAsync_CorreoDeOtroUsuarioEnMayusculas_LanzaReglaNegocioException()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: true);
        await _baseDatos.AgregarUsuarioAsync("laura@sondrive.com", Clave, Roles.Conductor, estado: true);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ActualizarConductorAsync(conductor.Id, DatosValidos("LAURA@SonDrive.com")));
    }

    [Fact]
    public async Task ActualizarConductorAsync_MismoCorreoPropio_Guarda()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: true);

        var respuesta = await CrearServicio().ActualizarConductorAsync(conductor.Id, DatosValidos("carlos@sondrive.com"));

        Assert.Equal("carlos@sondrive.com", respuesta.Correo);
        Assert.Equal("Carla", (await LeerUsuarioAsync(conductor.Id)).Nombre);
    }

    [Fact]
    public async Task ActualizarConductorAsync_UsuarioAdministrador_LanzaReglaNegocioException()
    {
        var administrador = await _baseDatos.AgregarUsuarioAsync("admin@sondrive.com", Clave, Roles.Administrador, estado: true);

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ActualizarConductorAsync(administrador.Id, DatosValidos()));

        Assert.Equal("Nombre", (await LeerUsuarioAsync(administrador.Id)).Nombre);
    }

    [Fact]
    public async Task ActualizarConductorAsync_ConductorDeshabilitado_ConservaEstadoRolYClave()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: false);

        var respuesta = await CrearServicio().ActualizarConductorAsync(conductor.Id, DatosValidos());

        Assert.False(respuesta.Estado);
        Assert.Equal(Roles.Conductor, respuesta.Rol);
        var guardado = await LeerUsuarioAsync(conductor.Id);
        Assert.False(guardado.Estado);
        Assert.Equal(Roles.Conductor, guardado.Rol);
        Assert.Equal(conductor.Clave, guardado.Clave);
    }

    [Fact]
    public async Task ActualizarConductorAsync_TextosConEspaciosYCorreoEnMayusculas_GuardaRecortadoYEnMinusculas()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: true);
        var dto = new ConductorActualizarDto
        {
            Nombre = "  Carla  ",
            Apellido = " Gómez ",
            Correo = "  Carla.Gomez@SonDrive.com ",
            Celular = " 3109876543 ",
            Foto = " https://ejemplo.com/carla.png "
        };

        await CrearServicio().ActualizarConductorAsync(conductor.Id, dto);

        var guardado = await LeerUsuarioAsync(conductor.Id);
        Assert.Equal("Carla", guardado.Nombre);
        Assert.Equal("Gómez", guardado.Apellido);
        Assert.Equal("carla.gomez@sondrive.com", guardado.Correo);
        Assert.Equal("3109876543", guardado.Celular);
        Assert.Equal("https://ejemplo.com/carla.png", guardado.Foto);
    }

    [Fact]
    public async Task ActualizarConductorAsync_FotoEnBlanco_GuardaNull()
    {
        var conductor = await _baseDatos.AgregarUsuarioAsync("carlos@sondrive.com", Clave, Roles.Conductor, estado: true);
        var dto = DatosValidos();
        dto.Foto = "   ";

        var respuesta = await CrearServicio().ActualizarConductorAsync(conductor.Id, dto);

        Assert.Null(respuesta.Foto);
        Assert.Null((await LeerUsuarioAsync(conductor.Id)).Foto);
    }

    public void Dispose() => _baseDatos.Dispose();
}
