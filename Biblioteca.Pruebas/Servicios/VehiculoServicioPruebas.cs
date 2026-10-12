using AutoMapper;
using Biblioteca.DTO.Vehiculo;
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

public sealed class VehiculoServicioPruebas : IDisposable
{
    private const string Clave = "Clave12345*";

    private readonly BaseDatosEnMemoria _baseDatos = new();

    private PuestoServicio CrearPuestoServicio()
    {
        var conexion = _baseDatos.CrearConexion();
        return new PuestoServicio(new PuestoRepositorio(conexion), new VehiculoRepositorio(conexion));
    }

    private VehiculoServicio CrearServicio()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<VehiculoProfile>(), NullLoggerFactory.Instance)
            .CreateMapper();

        var conexion = _baseDatos.CrearConexion();
        var vehiculos = new VehiculoRepositorio(conexion);
        return new VehiculoServicio(
            vehiculos,
            new UsuarioRepositorio(conexion),
            mapper,
            new PuestoServicio(new PuestoRepositorio(conexion), vehiculos));
    }

    private Task<Usuario> AgregarConductorAsync(string correo, bool estado = true) =>
        _baseDatos.AgregarUsuarioAsync(correo, Clave, Roles.Conductor, estado);

    private static VehiculoDatosDTO DatosValidos(long idUsuario, string placa = "ABC123") => new()
    {
        IdUsuario = idUsuario,
        Placa = placa,
        Modelo = "Mazda 3",
        AnnoModelo = "2024",
        Tipo = "Hatchback",
        Kilometraje = 49500,
        FechaUltimoServicio = DateTime.Today.AddDays(-10),
        VencimientoSoat = DateTime.Today.AddDays(200),
        VencimientoTecnomecanica = DateTime.Today.AddDays(300),
        VencimientoLicencia = DateTime.Today.AddDays(400)
    };

    private async Task<Vehiculo> LeerVehiculoAsync(long id)
    {
        await using var contexto = _baseDatos.CrearConexion();
        return await contexto.Vehiculos
            .AsNoTracking()
            .Include(v => v.Fallas)
            .Include(v => v.Puestos)
            .SingleAsync(v => v.Id == id);
    }

    [Fact]
    public async Task ActualizarAsync_DatosValidos_GuardaLosDiezDatos()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");
        var dto = DatosValidos(conductor.Id, "XYZ789");

        var respuesta = await CrearServicio().ActualizarAsync(vehiculo.Id, dto);

        Assert.Equal(vehiculo.Id, respuesta.Id);
        Assert.Equal("XYZ789", respuesta.Placa);
        Assert.Equal("Mazda 3 2024", respuesta.NombreVehiculo);

        var guardado = await LeerVehiculoAsync(vehiculo.Id);
        Assert.Equal(conductor.Id, guardado.IdUsuario);
        Assert.Equal("XYZ789", guardado.Placa);
        Assert.Equal("Mazda 3", guardado.Modelo);
        Assert.Equal("2024", guardado.AnnoModelo);
        Assert.Equal("Hatchback", guardado.Tipo);
        Assert.Equal(49500, guardado.Kilometraje);
        Assert.Equal(dto.FechaUltimoServicio, guardado.FechaUltimoServicio);
        Assert.Equal(dto.VencimientoSoat, guardado.VencimientoSoat);
        Assert.Equal(dto.VencimientoTecnomecanica, guardado.VencimientoTecnomecanica);
        Assert.Equal(dto.VencimientoLicencia, guardado.VencimientoLicencia);
    }

    [Fact]
    public async Task ActualizarAsync_VehiculoInexistente_LanzaNoEncontradoException()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");

        await Assert.ThrowsAsync<NoEncontradoException>(
            () => CrearServicio().ActualizarAsync(9999, DatosValidos(conductor.Id)));
    }

    [Fact]
    public async Task ActualizarAsync_PlacaDeOtroVehiculo_LanzaReglaNegocioException()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");
        await _baseDatos.AgregarVehiculoAsync(conductor.Id, "XYZ789");

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(conductor.Id, "xyz 789")));

        Assert.Equal("ABC123", (await LeerVehiculoAsync(vehiculo.Id)).Placa);
    }

    [Fact]
    public async Task ActualizarAsync_MismaPlacaEnMinusculasYConEspacio_GuardaNormalizada()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");

        var respuesta = await CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(conductor.Id, "abc 123"));

        Assert.Equal("ABC123", respuesta.Placa);
        var guardado = await LeerVehiculoAsync(vehiculo.Id);
        Assert.Equal("ABC123", guardado.Placa);
        Assert.Equal("Mazda 3", guardado.Modelo);
    }

    [Fact]
    public async Task ActualizarAsync_ConductorNuevoHabilitado_ReasignaElVehiculo()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var nuevo = await AgregarConductorAsync("laura@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");

        var respuesta = await CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(nuevo.Id));

        Assert.Equal(nuevo.Id, respuesta.IdUsuario);
        Assert.Equal(nuevo.Id, (await LeerVehiculoAsync(vehiculo.Id)).IdUsuario);
    }

    [Fact]
    public async Task ActualizarAsync_ConductorNuevoDeshabilitado_LanzaReglaNegocioException()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var deshabilitado = await AgregarConductorAsync("laura@sondrive.com", estado: false);
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(deshabilitado.Id)));

        Assert.Equal(conductor.Id, (await LeerVehiculoAsync(vehiculo.Id)).IdUsuario);
    }

    [Fact]
    public async Task ActualizarAsync_UsuarioNuevoAdministrador_LanzaReglaNegocioException()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var administrador = await _baseDatos.AgregarUsuarioAsync("admin@sondrive.com", Clave, Roles.Administrador, estado: true);
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");

        await Assert.ThrowsAsync<ReglaNegocioException>(
            () => CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(administrador.Id)));

        Assert.Equal(conductor.Id, (await LeerVehiculoAsync(vehiculo.Id)).IdUsuario);
    }

    [Fact]
    public async Task ActualizarAsync_UsuarioNuevoInexistente_LanzaNoEncontradoException()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");

        await Assert.ThrowsAsync<NoEncontradoException>(
            () => CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(9999)));
    }

    [Fact]
    public async Task ActualizarAsync_ConductorActualDeshabilitadoSinCambiarlo_Guarda()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com", estado: false);
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");

        var respuesta = await CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(conductor.Id));

        Assert.Equal(conductor.Id, respuesta.IdUsuario);
        Assert.Equal(49500, (await LeerVehiculoAsync(vehiculo.Id)).Kilometraje);
    }

    [Fact]
    public async Task ActualizarAsync_VehiculoConFallasYPuestos_NoLosAltera()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");
        var pendiente = await _baseDatos.AgregarFallaAsync(vehiculo.Id, "Alerta", estaResuelta: false);
        var resuelta = await _baseDatos.AgregarFallaAsync(vehiculo.Id, "Crítico", estaResuelta: true);
        await CrearPuestoServicio().AsegurarMapaAsync(vehiculo.Id);
        var antes = await LeerVehiculoAsync(vehiculo.Id);

        var respuesta = await CrearServicio().ActualizarAsync(vehiculo.Id, DatosValidos(conductor.Id));

        var despues = await LeerVehiculoAsync(vehiculo.Id);
        Assert.NotEmpty(antes.Puestos);
        Assert.Equal(antes.Puestos.Count, despues.Puestos.Count);
        Assert.Equal(2, despues.Fallas.Count);
        Assert.False(despues.Fallas.Single(f => f.Id == pendiente.Id).EstaResuelta);
        Assert.True(despues.Fallas.Single(f => f.Id == resuelta.Id).EstaResuelta);
        Assert.Equal(1, respuesta.FallasPendientes);
        Assert.Equal("Alerta", respuesta.Estado);
    }

    [Fact]
    public async Task ActualizarAsync_VencimientoPasado_LoAcepta()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");
        var dto = DatosValidos(conductor.Id);
        dto.VencimientoSoat = DateTime.Today.AddDays(-5);

        var respuesta = await CrearServicio().ActualizarAsync(vehiculo.Id, dto);

        Assert.Equal(-5, respuesta.Soat.DiasRestantes);
        Assert.Equal(DateTime.Today.AddDays(-5), (await LeerVehiculoAsync(vehiculo.Id)).VencimientoSoat);
    }

    [Fact]
    public async Task ActualizarAsync_KilometrajeMenorQueElAnterior_LoAcepta()
    {
        var conductor = await AgregarConductorAsync("carlos@sondrive.com");
        var vehiculo = await _baseDatos.AgregarVehiculoAsync(conductor.Id, "ABC123");
        var dto = DatosValidos(conductor.Id);
        dto.Kilometraje = 500;

        await CrearServicio().ActualizarAsync(vehiculo.Id, dto);

        Assert.Equal(500, (await LeerVehiculoAsync(vehiculo.Id)).Kilometraje);
    }

    public void Dispose() => _baseDatos.Dispose();
}
