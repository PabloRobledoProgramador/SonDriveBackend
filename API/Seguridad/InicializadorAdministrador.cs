using Biblioteca.DTO.Usuario;
using Biblioteca.Seguridad;
using Biblioteca.Servicios.Interfaces;

namespace API.Seguridad
{
    // Al arrancar, si no existe ningún administrador, crea uno con los datos de la sección
    // "AdministradorInicial". Así siempre hay alguien que pueda iniciar sesión y crear usuarios.
    public static class InicializadorAdministrador
    {
        public const string Seccion = "AdministradorInicial";

        public static async Task CrearAdministradorInicialAsync(this WebApplication app)
        {
            using var alcance = app.Services.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<IUsuarioServicio>();
            var registro = alcance.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
            var datos = app.Configuration.GetSection(Seccion).Get<UsuarioCrearDto>();

            try
            {
                if (await servicio.ExisteRolAsync(Roles.Administrador))
                    return;

                if (datos is null || string.IsNullOrWhiteSpace(datos.Correo) || string.IsNullOrWhiteSpace(datos.Clave))
                {
                    registro.LogWarning(
                        "No existe ningún administrador y no se configuró la sección {Seccion}; nadie podrá crear usuarios.",
                        Seccion);
                    return;
                }

                datos.Rol = Roles.Administrador;
                datos.Estado = true;
                await servicio.CrearAsync(datos);

                registro.LogInformation("Se creó el administrador inicial {Correo}.", datos.Correo);
            }
            catch (Exception ex)
            {
                // No se detiene la aplicación (por ejemplo, si aún no se aplicaron las migraciones)
                registro.LogError(ex, "No fue posible crear el administrador inicial.");
            }
        }
    }
}
