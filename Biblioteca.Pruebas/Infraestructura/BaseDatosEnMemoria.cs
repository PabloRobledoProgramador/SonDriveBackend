using Biblioteca.Modelo;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Pruebas.Infraestructura;

// Alias dentro del namespace: "Conexion" a secas resolvería al namespace Biblioteca.Conexion
using ConexionSonDrive = Biblioteca.Conexion.Conexion;

// Base de datos SQLite en memoria con el esquema real de Conexion. Vive mientras la conexión
// esté abierta, así que cada prueba crea la suya y la libera al terminar.
public sealed class BaseDatosEnMemoria : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<ConexionSonDrive> _opciones;

    public BaseDatosEnMemoria()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        _opciones = new DbContextOptionsBuilder<ConexionSonDrive>()
            .UseSqlite(_conexion)
            .Options;

        using var contexto = CrearConexion();
        contexto.Database.EnsureCreated();
    }

    // Nuevo contexto sobre la misma base de datos
    public ConexionSonDrive CrearConexion() => new(_opciones);

    public async Task<Usuario> AgregarUsuarioAsync(string correo, string clave, string rol, bool estado)
    {
        var usuario = new Usuario
        {
            Nombre = "Nombre",
            Apellido = "Apellido",
            Correo = correo,
            Clave = BCrypt.Net.BCrypt.HashPassword(clave),
            Celular = "3001234567",
            Estado = estado,
            Rol = rol
        };

        await using var contexto = CrearConexion();
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        return usuario;
    }

    public async Task<Vehiculo> AgregarVehiculoAsync(long idUsuario, string placa)
    {
        var vigencia = DateTime.Today.AddYears(1);
        var vehiculo = new Vehiculo
        {
            IdUsuario = idUsuario,
            Placa = placa,
            Modelo = "Kia Rio",
            AnnoModelo = "2022",
            Tipo = "Sedan",
            Kilometraje = 1000,
            VencimientoSoat = vigencia,
            VencimientoTecnomecanica = vigencia,
            VencimientoLicencia = vigencia
        };

        await using var contexto = CrearConexion();
        contexto.Vehiculos.Add(vehiculo);
        await contexto.SaveChangesAsync();
        return vehiculo;
    }

    public async Task<Falla> AgregarFallaAsync(long vehiculoId, string severidad, bool estaResuelta)
    {
        var falla = new Falla
        {
            VehiculoId = vehiculoId,
            Descripcion = "Falla de prueba",
            Severidad = severidad,
            Fecha = DateTime.Today,
            EstaResuelta = estaResuelta
        };

        await using var contexto = CrearConexion();
        contexto.Fallas.Add(falla);
        await contexto.SaveChangesAsync();
        return falla;
    }

    public void Dispose() => _conexion.Dispose();
}
