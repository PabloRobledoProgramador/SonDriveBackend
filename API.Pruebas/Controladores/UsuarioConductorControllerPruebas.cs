using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Pruebas.Infraestructura;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.DTO.Usuario;
using Biblioteca.Modelo;
using Biblioteca.Seguridad;

namespace API.Pruebas.Controladores;

public class UsuarioConductorControllerPruebas : IClassFixture<FabricaApiSonDrive>
{
    private const string Clave = "Clave12345*";
    private readonly FabricaApiSonDrive _fabrica;

    public UsuarioConductorControllerPruebas(FabricaApiSonDrive fabrica) =>
        _fabrica = fabrica;

    // La base se comparte dentro de la clase: cada prueba usa correos distintos
    private static string CorreoUnico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}@sondrive.com";

    private static string Ruta(long id) => $"/api/usuario/{id}/conductor";

    private Task<Usuario> SembrarConductorAsync(bool estado = true) =>
        _fabrica.SembrarUsuarioAsync(CorreoUnico("conductor"), Clave, Roles.Conductor, estado);

    private async Task<HttpClient> CrearClienteAdministradorAsync()
    {
        var administrador = await _fabrica.SembrarUsuarioAsync(CorreoUnico("admin"), Clave, Roles.Administrador);
        return await _fabrica.CrearClienteAutenticadoAsync(administrador.Correo, Clave);
    }

    private static ConductorActualizarDto DatosValidos(string? correo = null) => new()
    {
        Nombre = "Carla",
        Apellido = "Gómez",
        Correo = correo ?? CorreoUnico("editado"),
        Celular = "3109876543",
        Foto = "https://ejemplo.com/carla.png"
    };

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta)
    {
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    [Fact]
    public async Task ActualizarConductor_AdministradorConDatosValidos_Devuelve200ConLosDatosNuevos()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var dto = DatosValidos();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), dto);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var usuario = await LeerJsonAsync(respuesta);
        Assert.Equal(conductor.Id, usuario.GetProperty("id").GetInt64());
        Assert.Equal("Carla", usuario.GetProperty("nombre").GetString());
        Assert.Equal("Gómez", usuario.GetProperty("apellido").GetString());
        Assert.Equal(dto.Correo, usuario.GetProperty("correo").GetString());
        Assert.Equal("3109876543", usuario.GetProperty("celular").GetString());
        Assert.Equal("https://ejemplo.com/carla.png", usuario.GetProperty("foto").GetString());
        Assert.Equal(Roles.Conductor, usuario.GetProperty("rol").GetString());
        Assert.True(usuario.GetProperty("estado").GetBoolean());
        Assert.False(usuario.TryGetProperty("clave", out _));
    }

    [Fact]
    public async Task ActualizarConductor_DatosInvalidos_Devuelve400ConErroresPorCampo()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), new ConductorActualizarDto
        {
            Nombre = "",
            Apellido = "Gómez",
            Correo = "correo-no-valido",
            Celular = "123",
            Foto = "abc"
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var errores = (await LeerJsonAsync(respuesta)).GetProperty("errors");
        Assert.True(errores.TryGetProperty("Nombre", out _));
        Assert.True(errores.TryGetProperty("Correo", out _));
        Assert.True(errores.TryGetProperty("Celular", out _));
        Assert.True(errores.TryGetProperty("Foto", out _));
    }

    [Fact]
    public async Task ActualizarConductor_UsuarioInexistente_Devuelve404()
    {
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(999_999), DatosValidos());

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task ActualizarConductor_CorreoDeOtroUsuario_Devuelve409()
    {
        var conductor = await SembrarConductorAsync();
        var otro = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), DatosValidos(otro.Correo));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await LeerJsonAsync(respuesta);
        Assert.Contains("Ya existe otro usuario con el correo", problema.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ActualizarConductor_SobreUnAdministrador_Devuelve409()
    {
        var administrador = await _fabrica.SembrarUsuarioAsync(CorreoUnico("otro-admin"), Clave, Roles.Administrador);
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(administrador.Id), DatosValidos());

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await LeerJsonAsync(respuesta);
        Assert.Equal(
            "Solo se pueden editar por esta vía las cuentas de conductor.",
            problema.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ActualizarConductor_SinToken_Devuelve401()
    {
        var conductor = await SembrarConductorAsync();

        var respuesta = await _fabrica.CreateClient().PutAsJsonAsync(Ruta(conductor.Id), DatosValidos());

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task ActualizarConductor_ConductorSobreOtroConductor_Devuelve403()
    {
        var conductor = await SembrarConductorAsync();
        var otro = await SembrarConductorAsync();
        var cliente = await _fabrica.CrearClienteAutenticadoAsync(conductor.Correo, Clave);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(otro.Id), DatosValidos());

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task ActualizarConductor_ConductorSobreSiMismo_Devuelve403()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await _fabrica.CrearClienteAutenticadoAsync(conductor.Correo, Clave);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), DatosValidos());

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task ActualizarConductor_ConductorDeshabilitado_SigueDeshabilitado()
    {
        var conductor = await SembrarConductorAsync(estado: false);
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), DatosValidos());

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.False((await LeerJsonAsync(respuesta)).GetProperty("estado").GetBoolean());
    }

    [Fact]
    public async Task ActualizarConductor_CuerpoConEstadoYRol_LosIgnora()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), new
        {
            nombre = "Carla",
            apellido = "Gómez",
            correo = CorreoUnico("editado"),
            celular = "3109876543",
            foto = (string?)null,
            estado = false,
            rol = Roles.Administrador,
            clave = "OtraClave123*"
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var usuario = await LeerJsonAsync(respuesta);
        Assert.True(usuario.GetProperty("estado").GetBoolean());
        Assert.Equal(Roles.Conductor, usuario.GetProperty("rol").GetString());
    }

    [Fact]
    public async Task ActualizarConductor_CorreoNuevo_PermiteIniciarSesionConLaClaveAnterior()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var dto = DatosValidos();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), dto);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        var inicioSesion = await _fabrica.CreateClient().PostAsJsonAsync(
            "/api/autenticacion/iniciar-sesion",
            new InicioSesionDTO { Correo = dto.Correo, Clave = Clave });
        Assert.Equal(HttpStatusCode.OK, inicioSesion.StatusCode);
    }

    [Fact]
    public async Task ActualizarConductor_TextosConEspacios_LosRecortaAntesDeValidar()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var dto = DatosValidos();
        dto.Nombre = "  Ana  ";
        dto.Celular = " 3001234567 ";
        dto.Foto = "   ";

        var respuesta = await cliente.PutAsJsonAsync(Ruta(conductor.Id), dto);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var usuario = await LeerJsonAsync(respuesta);
        Assert.Equal("Ana", usuario.GetProperty("nombre").GetString());
        Assert.Equal("3001234567", usuario.GetProperty("celular").GetString());
        Assert.Equal(JsonValueKind.Null, usuario.GetProperty("foto").ValueKind);
    }
}
