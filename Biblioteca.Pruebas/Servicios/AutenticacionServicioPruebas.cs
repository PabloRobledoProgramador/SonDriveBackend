using AutoMapper;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.Excepciones;
using Biblioteca.Mapeo;
using Biblioteca.Pruebas.Infraestructura;
using Biblioteca.Repositorios.Implementaciones;
using Biblioteca.Seguridad;
using Biblioteca.Servicios.Implementaciones;
using Microsoft.Extensions.Logging.Abstractions;

namespace Biblioteca.Pruebas.Servicios;

public sealed class AutenticacionServicioPruebas : IDisposable
{
    private const string Clave = "Clave12345*";

    private readonly BaseDatosEnMemoria _baseDatos = new();
    private readonly GeneradorTokenFalso _generadorToken = new();

    private AutenticacionServicio CrearServicio()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<UsuarioProfile>(), NullLoggerFactory.Instance)
            .CreateMapper();
        return new AutenticacionServicio(new UsuarioRepositorio(_baseDatos.CrearConexion()), _generadorToken, mapper);
    }

    [Fact]
    public async Task IniciarSesionAsync_CredencialesCorrectas_DevuelveTokenYUsuario()
    {
        var usuario = await _baseDatos.AgregarUsuarioAsync("conductor@sondrive.com", Clave, Roles.Conductor, estado: true);

        var respuesta = await CrearServicio().IniciarSesionAsync(
            new InicioSesionDTO { Correo = "conductor@sondrive.com", Clave = Clave });

        Assert.Equal(GeneradorTokenFalso.Token, respuesta.Token);
        Assert.Equal(GeneradorTokenFalso.Expiracion, respuesta.Expiracion);
        Assert.Equal(usuario.Id, respuesta.Usuario.Id);
        Assert.Equal(Roles.Conductor, respuesta.Usuario.Rol);
        Assert.Equal(usuario.Id, _generadorToken.UltimoUsuario?.Id);
    }

    [Fact]
    public async Task IniciarSesionAsync_CorreoConMayusculasYEspacios_IniciaSesion()
    {
        await _baseDatos.AgregarUsuarioAsync("conductor@sondrive.com", Clave, Roles.Conductor, estado: true);

        var respuesta = await CrearServicio().IniciarSesionAsync(
            new InicioSesionDTO { Correo = "  Conductor@SonDrive.COM ", Clave = Clave });

        Assert.Equal("conductor@sondrive.com", respuesta.Usuario.Correo);
    }

    [Fact]
    public async Task IniciarSesionAsync_CorreoInexistente_LanzaCredencialesInvalidas()
    {
        var error = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            CrearServicio().IniciarSesionAsync(new InicioSesionDTO { Correo = "nadie@sondrive.com", Clave = Clave }));

        Assert.Equal("Correo o clave incorrectos.", error.Message);
    }

    [Fact]
    public async Task IniciarSesionAsync_ClaveIncorrecta_LanzaMismoMensajeQueCorreoInexistente()
    {
        await _baseDatos.AgregarUsuarioAsync("conductor@sondrive.com", Clave, Roles.Conductor, estado: true);
        var servicio = CrearServicio();

        var errorClave = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            servicio.IniciarSesionAsync(new InicioSesionDTO { Correo = "conductor@sondrive.com", Clave = "otra-clave" }));
        var errorCorreo = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            servicio.IniciarSesionAsync(new InicioSesionDTO { Correo = "nadie@sondrive.com", Clave = Clave }));

        // No se revela cuál de los dos datos falló
        Assert.Equal(errorCorreo.Message, errorClave.Message);
    }

    [Fact]
    public async Task IniciarSesionAsync_UsuarioDesactivado_LanzaCredencialesInvalidas()
    {
        await _baseDatos.AgregarUsuarioAsync("inactivo@sondrive.com", Clave, Roles.Conductor, estado: false);

        var error = await Assert.ThrowsAsync<CredencialesInvalidasException>(() =>
            CrearServicio().IniciarSesionAsync(new InicioSesionDTO { Correo = "inactivo@sondrive.com", Clave = Clave }));

        Assert.Equal("El usuario está desactivado. Contacta al administrador.", error.Message);
        Assert.Null(_generadorToken.UltimoUsuario);
    }

    public void Dispose() => _baseDatos.Dispose();
}
