using API.Middleware;
using API.Validaciones.Usuario;
using Biblioteca.Conexion;
using Biblioteca.Mapeo;
using Biblioteca.Repositorios.Implementaciones;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Implementaciones;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la Base de Datos con EF Core
builder.Services.AddDbContext<Conexion>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("string_conexion")));

// 2. Registro de IConexion apuntando al mismo DbContext
builder.Services.AddScoped<IConexion>(provider => provider.GetRequiredService<Conexion>());

//3. Registro de AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(UsuarioProfile));

//4. Registro de FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UsuarioCrearDtoValidador>();
builder.Services.AddValidatorsFromAssemblyContaining<UsuarioActualizarDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UsuarioCambiarClaveDtoValidator>();

// AddScoped para los servicios de la capa de negocio
builder.Services.AddScoped<ICiudadServicio, CiudadServicio>();
builder.Services.AddScoped<IHoraServicio, HoraServicio>();
builder.Services.AddScoped<IFallaServicio, FallaServicio>();
builder.Services.AddScoped<IViajeServicio, ViajeServicio>();
builder.Services.AddScoped<IVehiculoServicio, VehiculoServicio>();
builder.Services.AddScoped<IOcupacionServicio, OcupacionServicio>();
builder.Services.AddScoped<IRutaServicio, RutaServicio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();

//AddScoped para los repositorios
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();

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

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SonDrive API v1");
    });
}

// Agregar el middleware de manejo de excepciones
app.UseMiddleware<ManejoExcepcionesMiddleware>();

app.UseHttpsRedirection();

// Habilitar la política de CORS
app.UseCors("PermitirFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();