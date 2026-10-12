using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Pruebas.Infraestructura;
using Biblioteca.DTO.Usuario;
using Biblioteca.DTO.Vehiculo;
using Biblioteca.Modelo;
using Biblioteca.Seguridad;

namespace API.Pruebas.Controladores;

public class VehiculoEdicionControllerPruebas : IClassFixture<FabricaApiSonDrive>
{
    private const string Clave = "Clave12345*";

    private readonly FabricaApiSonDrive _fabrica;

    public VehiculoEdicionControllerPruebas(FabricaApiSonDrive fabrica) =>
        _fabrica = fabrica;

    // La base se comparte dentro de la clase: cada prueba usa correos y placas distintos
    private static string CorreoUnico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}@sondrive.com";

    private static string PlacaUnica()
    {
        var letras = new string(Enumerable.Range(0, 3).Select(_ => (char)('A' + Random.Shared.Next(26))).ToArray());
        return $"{letras}{Random.Shared.Next(1000):D3}";
    }

    private static string Ruta(long id) => $"/api/vehiculo/{id}";

    private Task<Usuario> SembrarConductorAsync(bool estado = true) =>
        _fabrica.SembrarUsuarioAsync(CorreoUnico("conductor"), Clave, Roles.Conductor, estado);

    private async Task<HttpClient> CrearClienteAdministradorAsync()
    {
        var administrador = await _fabrica.SembrarUsuarioAsync(CorreoUnico("admin"), Clave, Roles.Administrador);
        return await _fabrica.CrearClienteAutenticadoAsync(administrador.Correo, Clave);
    }

    private static VehiculoDatosDTO DatosValidos(long idUsuario, string? placa = null) => new()
    {
        IdUsuario = idUsuario,
        Placa = placa ?? PlacaUnica(),
        Modelo = "Kia Rio",
        AnnoModelo = "2022",
        Tipo = "Sedan",
        Kilometraje = 48200,
        FechaUltimoServicio = DateTime.Today.AddDays(-30),
        VencimientoSoat = DateTime.Today.AddDays(200),
        VencimientoTecnomecanica = DateTime.Today.AddDays(300),
        VencimientoLicencia = DateTime.Today.AddDays(400)
    };

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta)
    {
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    private static async Task<long> CrearVehiculoAsync(HttpClient administrador, long idUsuario, string? placa = null)
    {
        var respuesta = await administrador.PostAsJsonAsync("/api/vehiculo", DatosValidos(idUsuario, placa));
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return (await LeerJsonAsync(respuesta)).GetProperty("id").GetInt64();
    }

    [Fact]
    public async Task Actualizar_AdministradorConDatosValidos_Devuelve200ConLosDatosNuevos()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(cliente, conductor.Id);
        var placa = PlacaUnica();
        var dto = DatosValidos(conductor.Id, $"{placa[..3].ToLowerInvariant()} {placa[3..]}");
        dto.Modelo = "Mazda 3";
        dto.AnnoModelo = "2024";
        dto.Kilometraje = 49500;
        dto.VencimientoSoat = DateTime.Today.AddDays(10);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), dto);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var vehiculo = await LeerJsonAsync(respuesta);
        Assert.Equal(id, vehiculo.GetProperty("id").GetInt64());
        Assert.Equal(placa, vehiculo.GetProperty("placa").GetString());
        Assert.Equal("Mazda 3 2024", vehiculo.GetProperty("nombreVehiculo").GetString());
        Assert.Equal(49500, vehiculo.GetProperty("kilometraje").GetInt32());
        Assert.Equal(conductor.Id, vehiculo.GetProperty("idUsuario").GetInt64());
        Assert.Equal("Óptimo", vehiculo.GetProperty("estado").GetString());
        Assert.Equal(10, vehiculo.GetProperty("soat").GetProperty("diasRestantes").GetInt32());
    }

    [Fact]
    public async Task Actualizar_DatosInvalidos_Devuelve400ConErroresPorCampo()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(cliente, conductor.Id);
        var dto = DatosValidos(conductor.Id, "AB12");
        dto.Modelo = "";
        dto.Kilometraje = -5;

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), dto);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var errores = (await LeerJsonAsync(respuesta)).GetProperty("errors");
        Assert.True(errores.TryGetProperty("Placa", out _));
        Assert.True(errores.TryGetProperty("Modelo", out _));
        Assert.True(errores.TryGetProperty("Kilometraje", out _));
    }

    [Fact]
    public async Task Actualizar_VehiculoInexistente_Devuelve404()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();

        var respuesta = await cliente.PutAsJsonAsync(Ruta(999_999), DatosValidos(conductor.Id));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_PlacaDeOtroVehiculo_Devuelve409()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(cliente, conductor.Id);
        var placaDeOtro = PlacaUnica();
        await CrearVehiculoAsync(cliente, conductor.Id, placaDeOtro);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), DatosValidos(conductor.Id, placaDeOtro));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await LeerJsonAsync(respuesta);
        Assert.Contains("Ya existe otro vehículo con la placa", problema.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Actualizar_ReasignarAConductorDeshabilitado_Devuelve409()
    {
        var conductor = await SembrarConductorAsync();
        var deshabilitado = await SembrarConductorAsync(estado: false);
        var cliente = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(cliente, conductor.Id);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), DatosValidos(deshabilitado.Id));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await LeerJsonAsync(respuesta);
        Assert.Equal("El conductor seleccionado está inactivo.", problema.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Actualizar_ReasignarAUnAdministrador_Devuelve409()
    {
        var conductor = await SembrarConductorAsync();
        var administrador = await _fabrica.SembrarUsuarioAsync(CorreoUnico("otro-admin"), Clave, Roles.Administrador);
        var cliente = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(cliente, conductor.Id);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), DatosValidos(administrador.Id));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        var problema = await LeerJsonAsync(respuesta);
        Assert.Equal(
            "El usuario seleccionado no tiene rol de conductor.",
            problema.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Actualizar_SinToken_Devuelve401()
    {
        var conductor = await SembrarConductorAsync();
        var administrador = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(administrador, conductor.Id);

        var respuesta = await _fabrica.CreateClient().PutAsJsonAsync(Ruta(id), DatosValidos(conductor.Id));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_ConductorSobreSuPropioVehiculo_Devuelve403()
    {
        var conductor = await SembrarConductorAsync();
        var administrador = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(administrador, conductor.Id);
        var cliente = await _fabrica.CrearClienteAutenticadoAsync(conductor.Correo, Clave);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), DatosValidos(conductor.Id));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_ConductorSobreElVehiculoDeOtro_Devuelve403()
    {
        var conductor = await SembrarConductorAsync();
        var otro = await SembrarConductorAsync();
        var administrador = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(administrador, otro.Id);
        var cliente = await _fabrica.CrearClienteAutenticadoAsync(conductor.Correo, Clave);

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), DatosValidos(otro.Id));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Actualizar_ConductorActualDeshabilitadoSinCambiarlo_Devuelve200()
    {
        var conductor = await SembrarConductorAsync();
        var cliente = await CrearClienteAdministradorAsync();
        var id = await CrearVehiculoAsync(cliente, conductor.Id);
        var deshabilitar = await cliente.PatchAsync($"/api/usuario/{conductor.Id}/estado?estado=false", null);
        Assert.Equal(HttpStatusCode.NoContent, deshabilitar.StatusCode);
        var dto = DatosValidos(conductor.Id);
        dto.Kilometraje = 60000;

        var respuesta = await cliente.PutAsJsonAsync(Ruta(id), dto);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var vehiculo = await LeerJsonAsync(respuesta);
        Assert.Equal(60000, vehiculo.GetProperty("kilometraje").GetInt32());
        Assert.Equal(conductor.Id, vehiculo.GetProperty("idUsuario").GetInt64());
    }

    [Fact]
    public async Task Actualizar_ReasignarVehiculoConViajeProximo_ElViajePasaAlNuevoConductorConSusPuestos()
    {
        var conductorA = await SembrarConductorAsync();
        var conductorB = await SembrarConductorAsync();
        var administrador = await CrearClienteAdministradorAsync();
        // Los usuarios sembrados comparten nombre y celular: B recibe los suyos para distinguirlo
        var datosB = await administrador.PutAsJsonAsync($"/api/usuario/{conductorB.Id}/conductor", new ConductorActualizarDto
        {
            Nombre = "Laura",
            Apellido = "Pérez",
            Correo = conductorB.Correo,
            Celular = "3119998877"
        });
        Assert.Equal(HttpStatusCode.OK, datosB.StatusCode);
        var placa = PlacaUnica();
        var idVehiculo = await CrearVehiculoAsync(administrador, conductorA.Id, placa);

        var clienteA = await _fabrica.CrearClienteAutenticadoAsync(conductorA.Correo, Clave);
        var viajeCreado = await clienteA.PostAsJsonAsync("/api/viaje", new
        {
            ciudadOrigenId = 1,
            ciudadDestinoId = 2,
            idVehiculo,
            fecha = DateTime.Today.AddDays(7),
            hora = "08:00:00",
            precio = 12000
        });
        Assert.Equal(HttpStatusCode.Created, viajeCreado.StatusCode);
        var idViaje = (await LeerJsonAsync(viajeCreado)).GetProperty("id").GetInt64();

        var detalleAntes = await LeerJsonAsync(await clienteA.GetAsync($"/api/viaje/{idViaje}"));
        var idPuesto = detalleAntes.GetProperty("puestos").EnumerateArray()
            .First(p => !p.GetProperty("esConductor").GetBoolean())
            .GetProperty("idPuesto").GetInt64();
        var ocupado = await clienteA.PostAsJsonAsync(
            $"/api/viaje/{idViaje}/ocupaciones/puesto/{idPuesto}",
            new { nombreCompleto = "Pedro Ruiz", celular = "3201112233" });
        Assert.Equal(HttpStatusCode.OK, ocupado.StatusCode);

        var respuesta = await administrador.PutAsJsonAsync(Ruta(idVehiculo), DatosValidos(conductorB.Id, placa));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("Laura Pérez", (await LeerJsonAsync(respuesta)).GetProperty("conductor").GetString());

        var viajesDeB = await LeerJsonAsync(await administrador.GetAsync($"/api/viaje/usuario/{conductorB.Id}"));
        Assert.Contains(viajesDeB.EnumerateArray(), v => v.GetProperty("id").GetInt64() == idViaje);

        var viajesDeA = await LeerJsonAsync(await administrador.GetAsync($"/api/viaje/usuario/{conductorA.Id}"));
        Assert.DoesNotContain(viajesDeA.EnumerateArray(), v => v.GetProperty("id").GetInt64() == idViaje);

        var detalle = await LeerJsonAsync(await _fabrica.CreateClient().GetAsync($"/api/viaje/{idViaje}"));
        Assert.Equal("Laura Pérez", detalle.GetProperty("conductor").GetString());
        Assert.Equal("3119998877", detalle.GetProperty("telefonoConductor").GetString());
        var puesto = detalle.GetProperty("puestos").EnumerateArray()
            .Single(p => p.GetProperty("idPuesto").GetInt64() == idPuesto);
        Assert.True(puesto.GetProperty("ocupado").GetBoolean());

        var ocupaciones = await LeerJsonAsync(await administrador.GetAsync($"/api/viaje/{idViaje}/ocupaciones"));
        var ocupacion = Assert.Single(ocupaciones.EnumerateArray());
        Assert.Equal(idPuesto, ocupacion.GetProperty("idPuesto").GetInt64());
        Assert.Equal("Pedro Ruiz", ocupacion.GetProperty("pasajero").GetProperty("nombreCompleto").GetString());
    }
}
