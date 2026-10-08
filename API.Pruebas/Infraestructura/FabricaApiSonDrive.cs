using System.Net.Http.Headers;
using System.Net.Http.Json;
using Biblioteca.Conexion;
using Biblioteca.DTO.InicioSesion;
using Biblioteca.Modelo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Pruebas.Infraestructura;

// Levanta la API real en memoria con SQLite en lugar de SQL Server: las pruebas no dependen de la
// base de datos local de ningún integrante.
public sealed class FabricaApiSonDrive : WebApplicationFactory<Program>
{
    public const string ClaveJwt = "clave-de-pruebas-sondrive-de-al-menos-32-caracteres";
    public const string OrigenPermitido = "http://localhost:5173";

    private readonly SqliteConnection _conexion;
    private readonly DbContextOptions<Conexion> _opciones;

    public FabricaApiSonDrive()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();
        _opciones = new DbContextOptionsBuilder<Conexion>().UseSqlite(_conexion).Options;

        // El esquema debe existir antes de que arranque el host: Program.cs consulta los usuarios
        // (CrearAdministradorInicialAsync) antes de app.Run().
        using var contexto = new Conexion(_opciones);
        contexto.Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Pruebas");

        // UseSetting se aplica antes de que Program.cs lea las secciones Jwt y Cors
        builder.UseSetting("Jwt:Clave", ClaveJwt);
        builder.UseSetting("Jwt:Emisor", "SonDriveAPI");
        builder.UseSetting("Jwt:Audiencia", "SonDriveFrontend");
        builder.UseSetting("Jwt:MinutosExpiracion", "60");
        builder.UseSetting("Cors:Origenes:0", OrigenPermitido);

        builder.ConfigureTestServices(servicios =>
        {
            // Desde EF Core 9 también hay que quitar IDbContextOptionsConfiguration: si no, quedan
            // registrados SQL Server y SQLite a la vez y EF falla.
            servicios.RemoveAll<DbContextOptions<Conexion>>();
            servicios.RemoveAll<IDbContextOptionsConfiguration<Conexion>>();
            servicios.AddDbContext<Conexion>(opciones => opciones.UseSqlite(_conexion));
        });
    }

    public async Task<Usuario> SembrarUsuarioAsync(string correo, string clave, string rol, bool estado = true)
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

        await using var contexto = new Conexion(_opciones);
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        return usuario;
    }

    // Inicia sesión por la API y devuelve un cliente que envía el token en cada solicitud
    public async Task<HttpClient> CrearClienteAutenticadoAsync(string correo, string clave)
    {
        var cliente = CreateClient();
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/autenticacion/iniciar-sesion", new InicioSesionDTO { Correo = correo, Clave = clave });
        respuesta.EnsureSuccessStatusCode();

        var sesion = await respuesta.Content.ReadFromJsonAsync<InicioSesionRespuestaDTO>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sesion!.Token);
        return cliente;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _conexion.Dispose();
    }
}
