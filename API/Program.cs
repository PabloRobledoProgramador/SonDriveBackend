using Biblioteca.Conexion;
using Biblioteca.Implementacion;
using Microsoft.EntityFrameworkCore;
using Biblioteca.Interfaz;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la Base de Datos con EF Core
builder.Services.AddDbContext<Conexion>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("string_conexion")));

// 2. Registro de IConexion apuntando al mismo DbContext
builder.Services.AddScoped<IConexion>(provider => provider.GetRequiredService<Conexion>());

// AddScoped para los servicios de la capa de negocio
builder.Services.AddScoped<ICiudad, CiudadServicio>();
builder.Services.AddScoped<IHora, HoraServicio>();
builder.Services.AddScoped<IFalla, FallaServicio>();
builder.Services.AddScoped<IViaje, ViajeServicio>();
builder.Services.AddScoped<IVehiculo, VehiculoServicio>();
builder.Services.AddScoped<IOcupacion, OcupacionServicio>();
builder.Services.AddScoped<IRuta, RutaServicio>();
builder.Services.AddScoped<IUsuario, UsuarioServicio>();

// 3. Configuración de CORS para permitir peticiones desde React (Frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Servicios base del contenedor (Controladores y OpenAPI)
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Habilitar la política de CORS
app.UseCors("PermitirFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();