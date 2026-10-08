using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using API.Pruebas.Infraestructura;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.Seguridad;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace API.Pruebas.Controladores;

// Endpoints de autenticación a través de HTTP (contracts/api-autenticacion.md).
// Matriz de acceso de "perfil": anónimo, Conductor y Administrador. El caso "conductor sobre datos
// ajenos" no aplica: "perfil" siempre devuelve al dueño del token.
public class AutenticacionControllerPruebas : IClassFixture<FabricaApiSonDrive>
{
    private const string RutaIniciarSesion = "/api/autenticacion/iniciar-sesion";
    private const string RutaPerfil = "/api/autenticacion/perfil";
    private const string Clave = "Clave12345*";

    private readonly FabricaApiSonDrive _fabrica;

    public AutenticacionControllerPruebas(FabricaApiSonDrive fabrica) => _fabrica = fabrica;

    // Cada prueba usa su propio correo porque la base de datos se comparte dentro de la clase
    private static string CorreoUnico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}@sondrive.com";

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta) =>
        JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement;

    private static void AfirmarProblemDetails(HttpResponseMessage respuesta, HttpStatusCode estado)
    {
        Assert.Equal(estado, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData(Roles.Conductor)]
    [InlineData(Roles.Administrador)]
    public async Task IniciarSesion_UsuarioActivo_Devuelve200ConTokenYUsuario(string rol)
    {
        var correo = CorreoUnico("activo");
        await _fabrica.SembrarUsuarioAsync(correo, Clave, rol);
        var cliente = _fabrica.CreateClient();

        var antes = DateTime.UtcNow;
        var respuesta = await cliente.PostAsJsonAsync(RutaIniciarSesion, new InicioSesionDTO { Correo = correo, Clave = Clave });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var json = await LeerJsonAsync(respuesta);
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("token").GetString()));
        Assert.Equal(rol, json.GetProperty("usuario").GetProperty("rol").GetString());
        Assert.False(json.GetProperty("usuario").TryGetProperty("clave", out _));

        // Comparación relativa: GeneradorTokenJwt (código existente) usa la hora real, y la fábrica
        // configura 60 minutos de vigencia.
        var expiracion = json.GetProperty("expiracion").GetDateTime().ToUniversalTime();
        Assert.InRange(expiracion, antes.AddMinutes(59), antes.AddMinutes(61));
    }

    [Fact]
    public async Task IniciarSesion_CorreoConMayusculas_Devuelve200()
    {
        var correo = CorreoUnico("mayusculas");
        await _fabrica.SembrarUsuarioAsync(correo, Clave, Roles.Conductor);
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaIniciarSesion,
            new InicioSesionDTO { Correo = "  " + correo.ToUpperInvariant() + " ", Clave = Clave });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task IniciarSesion_DatosInvalidos_Devuelve400ConErroresPorCampo()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaIniciarSesion, new InicioSesionDTO { Correo = "no-es-correo", Clave = "" });

        AfirmarProblemDetails(respuesta, HttpStatusCode.BadRequest);
        var errores = (await LeerJsonAsync(respuesta)).GetProperty("errors");
        Assert.True(errores.TryGetProperty("Correo", out _));
        Assert.True(errores.TryGetProperty("Clave", out _));
    }

    [Fact]
    public async Task IniciarSesion_CorreoInexistente_Devuelve401ProblemDetails()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaIniciarSesion,
            new InicioSesionDTO { Correo = CorreoUnico("nadie"), Clave = Clave });

        AfirmarProblemDetails(respuesta, HttpStatusCode.Unauthorized);
        Assert.Equal("Correo o clave incorrectos.", (await LeerJsonAsync(respuesta)).GetProperty("detail").GetString());
    }

    [Fact]
    public async Task IniciarSesion_ClaveErrada_Devuelve401ConMismoDetalle()
    {
        var correo = CorreoUnico("clave-errada");
        await _fabrica.SembrarUsuarioAsync(correo, Clave, Roles.Conductor);
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaIniciarSesion, new InicioSesionDTO { Correo = correo, Clave = "otra-clave" });

        AfirmarProblemDetails(respuesta, HttpStatusCode.Unauthorized);
        Assert.Equal("Correo o clave incorrectos.", (await LeerJsonAsync(respuesta)).GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Perfil_SinToken_Devuelve401ProblemDetails()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.GetAsync(RutaPerfil);

        AfirmarProblemDetails(respuesta, HttpStatusCode.Unauthorized);
        Assert.Equal(401, (await LeerJsonAsync(respuesta)).GetProperty("status").GetInt32());
    }

    [Theory]
    [InlineData(Roles.Conductor)]
    [InlineData(Roles.Administrador)]
    public async Task Perfil_TokenValido_Devuelve200ConUsuario(string rol)
    {
        var correo = CorreoUnico("perfil");
        var usuario = await _fabrica.SembrarUsuarioAsync(correo, Clave, rol);
        var cliente = await _fabrica.CrearClienteAutenticadoAsync(correo, Clave);

        var respuesta = await cliente.GetAsync(RutaPerfil);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var json = await LeerJsonAsync(respuesta);
        Assert.Equal(usuario.Id, json.GetProperty("id").GetInt64());
        Assert.Equal(rol, json.GetProperty("rol").GetString());
    }

    [Fact]
    public async Task Perfil_TokenFirmadoConOtraClave_Devuelve401()
    {
        var cliente = _fabrica.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenFirmadoConOtraClave());

        var respuesta = await cliente.GetAsync(RutaPerfil);

        AfirmarProblemDetails(respuesta, HttpStatusCode.Unauthorized);
    }

    // Token con emisor y audiencia correctos pero firmado con una clave distinta a la de la API
    private static string TokenFirmadoConOtraClave()
    {
        var llave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("otra-clave-distinta-de-al-menos-32-caracteres!!"));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "SonDriveAPI",
            Audience = "SonDriveFrontend",
            Expires = DateTime.UtcNow.AddMinutes(30),
            Claims = new Dictionary<string, object> { ["sub"] = "1", ["rol"] = Roles.Administrador },
            SigningCredentials = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    [Fact]
    public async Task IniciarSesion_UsuarioDesactivado_Devuelve401()
    {
        var correo = CorreoUnico("inactivo");
        await _fabrica.SembrarUsuarioAsync(correo, Clave, Roles.Conductor, estado: false);
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(RutaIniciarSesion, new InicioSesionDTO { Correo = correo, Clave = Clave });

        AfirmarProblemDetails(respuesta, HttpStatusCode.Unauthorized);
        Assert.Equal("El usuario está desactivado. Contacta al administrador.",
            (await LeerJsonAsync(respuesta)).GetProperty("detail").GetString());
    }
}
