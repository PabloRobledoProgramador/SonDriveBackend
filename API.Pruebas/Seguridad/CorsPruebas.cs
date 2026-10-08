using API.Pruebas.Infraestructura;

namespace API.Pruebas.Seguridad;

// La política PermitirFrontend solo admite los orígenes de "Cors:Origenes" (principio IV)
public class CorsPruebas : IClassFixture<FabricaApiSonDrive>
{
    private readonly FabricaApiSonDrive _fabrica;

    public CorsPruebas(FabricaApiSonDrive fabrica) => _fabrica = fabrica;

    private static HttpRequestMessage CrearPreflight(string origen)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Options, "/api/autenticacion/iniciar-sesion");
        solicitud.Headers.Add("Origin", origen);
        solicitud.Headers.Add("Access-Control-Request-Method", "POST");
        solicitud.Headers.Add("Access-Control-Request-Headers", "content-type");
        return solicitud;
    }

    [Fact]
    public async Task Preflight_OrigenPermitido_IncluyeCabeceraAllowOrigin()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.SendAsync(CrearPreflight(FabricaApiSonDrive.OrigenPermitido));

        Assert.True(respuesta.Headers.TryGetValues("Access-Control-Allow-Origin", out var valores));
        Assert.Equal(FabricaApiSonDrive.OrigenPermitido, Assert.Single(valores));
    }

    [Fact]
    public async Task Preflight_OrigenNoPermitido_SinCabeceraAllowOrigin()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.SendAsync(CrearPreflight("http://malicioso.example"));

        Assert.False(respuesta.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
