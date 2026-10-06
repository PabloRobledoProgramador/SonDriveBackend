using API.Middleware;
using API.Seguridad;
using API.Validaciones.Usuario;
using Biblioteca.Conexion;
using Biblioteca.Mapeo;
using Biblioteca.Repositorios.Implementaciones;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Implementaciones;
using Biblioteca.Servicios.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddScoped<IFallaServicio, FallaServicio>();
builder.Services.AddScoped<IViajeServicio, ViajeServicio>();
builder.Services.AddScoped<IVehiculoServicio, VehiculoServicio>();
builder.Services.AddScoped<IOcupacionServicio, OcupacionServicio>();
builder.Services.AddScoped<IRutaServicio, RutaServicio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();
builder.Services.AddScoped<IPuestoServicio, PuestoServicio>();
builder.Services.AddScoped<IAutenticacionServicio, AutenticacionServicio>();

//AddScoped para los repositorios
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<ICiudadRepositorio, CiudadRepositorio>();
builder.Services.AddScoped<IRutaRepositorio, RutaRepositorio>();
builder.Services.AddScoped<IFallaRepositorio, FallaRepositorio>();
builder.Services.AddScoped<IVehiculoRepositorio, VehiculoRepositorio>();
builder.Services.AddScoped<IViajeRepositorio, ViajeRepositorio>();
builder.Services.AddScoped<IPuestoRepositorio, PuestoRepositorio>();
builder.Services.AddScoped<IOcupacionRepositorio, OcupacionRepositorio>();

// 5. Autenticación con JWT
var seccionJwt = builder.Configuration.GetSection(ConfiguracionJwt.Seccion);
var configuracionJwt = seccionJwt.Get<ConfiguracionJwt>() ?? new ConfiguracionJwt();

if (configuracionJwt.Clave.Length < 32)
    throw new InvalidOperationException(
        "Falta configurar 'Jwt:Clave' (mínimo 32 caracteres). En desarrollo está en appsettings.Development.json; " +
        "en otros ambientes debe venir de una variable de entorno (Jwt__Clave) o de user secrets.");

builder.Services.Configure<ConfiguracionJwt>(seccionJwt);
builder.Services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Se usan los nombres de claims tal cual vienen en el token ("sub", "rol", ...)
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracionJwt.Emisor,
            ValidateAudience = true,
            ValidAudience = configuracionJwt.Audiencia,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GeneradorTokenJwt.CrearLlaveFirma(configuracionJwt.Clave),
            NameClaimType = NombresClaims.Nombre,
            RoleClaimType = NombresClaims.Rol
        };
    });

// 6. Autorización: por defecto TODO endpoint exige un token válido.
// Los públicos se marcan con [AllowAnonymous] y los restringidos por rol con [Authorize(Roles = ...)].
builder.Services.AddAuthorization(opciones =>
{
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// 7. Configuración de CORS para permitir peticiones desde React (Frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 8. Servicios base del contenedor (Controladores y OpenAPI)
builder.Services.AddControllers();
builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer<TransformadorSeguridadOpenApi>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Crea el primer administrador si todavía no existe ninguno
await app.CrearAdministradorInicialAsync();

app.Run();
