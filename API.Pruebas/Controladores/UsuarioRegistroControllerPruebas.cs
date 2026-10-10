using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Pruebas.Infraestructura;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.DTO.Usuario;
using Biblioteca.Seguridad;

namespace API.Pruebas.Controladores;

public class UsuarioRegistroControllerPruebas : IClassFixture<FabricaApiSonDrive>
{
    private const string RutaRegistro = "/api/usuario/registro";
    private readonly FabricaApiSonDrive _fabrica;

    public UsuarioRegistroControllerPruebas(FabricaApiSonDrive fabrica) =>
        _fabrica = fabrica;

    [Fact]
    public async Task Registrar_Anonimo_CreaConductorActivoYPermiteIniciarSesion()
    {
        const string clave = "Conductor123*";
        var dto = new UsuarioCrearDto
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Correo = $"registro-{Guid.NewGuid():N}@sondrive.com",
            Clave = clave,
            Celular = "3001234567"
        };
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaRegistro, dto);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var usuario = json.RootElement;
        Assert.Equal(dto.Nombre, usuario.GetProperty("nombre").GetString());
        Assert.Equal(dto.Apellido, usuario.GetProperty("apellido").GetString());
        Assert.Equal(dto.Correo, usuario.GetProperty("correo").GetString());
        Assert.Equal(Roles.Conductor, usuario.GetProperty("rol").GetString());
        Assert.True(usuario.GetProperty("estado").GetBoolean());
        Assert.False(usuario.TryGetProperty("clave", out _));

        var inicioSesion = await cliente.PostAsJsonAsync(
            "/api/autenticacion/iniciar-sesion",
            new InicioSesionDTO { Correo = dto.Correo, Clave = clave });
        Assert.Equal(HttpStatusCode.OK, inicioSesion.StatusCode);
    }

    [Fact]
    public async Task Registrar_DtoSolicitaRolAdministrador_SiempreCreaConductor()
    {
        var dto = new UsuarioCrearDto
        {
            Nombre = "Luis",
            Apellido = "Prueba",
            Correo = $"rol-registro-{Guid.NewGuid():N}@sondrive.com",
            Clave = "Conductor123*",
            Celular = "3001234567",
            Estado = false,
            Rol = Roles.Administrador,
            Foto = "https://ejemplo.invalid/foto.png"
        };
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaRegistro, dto);

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var usuario = json.RootElement;
        Assert.Equal(Roles.Conductor, usuario.GetProperty("rol").GetString());
        Assert.True(usuario.GetProperty("estado").GetBoolean());
        Assert.Equal(JsonValueKind.Null, usuario.GetProperty("foto").ValueKind);
    }

    [Fact]
    public async Task Registrar_DatosInvalidos_DevuelveErroresPorCampo()
    {
        var cliente = _fabrica.CreateClient();
        var respuesta = await cliente.PostAsJsonAsync(
            RutaRegistro,
            new UsuarioCrearDto
            {
                Nombre = "",
                Apellido = "",
                Correo = "correo-no-valido",
                Clave = "123",
                Celular = "123"
            });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var errores = json.RootElement.GetProperty("errors");
        Assert.True(errores.TryGetProperty("Nombre", out _));
        Assert.True(errores.TryGetProperty("Correo", out _));
        Assert.True(errores.TryGetProperty("Clave", out _));
        Assert.True(errores.TryGetProperty("Celular", out _));
    }

    [Fact]
    public async Task CrearUsuario_Normal_SigueRequiriendoAdministrador()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/usuario",
            new UsuarioCrearDto
            {
                Nombre = "Ana",
                Apellido = "Pérez",
                Correo = $"privado-{Guid.NewGuid():N}@sondrive.com",
                Clave = "Conductor123*",
                Celular = "3001234567"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }
}
