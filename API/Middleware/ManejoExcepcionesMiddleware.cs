using Biblioteca.Excepciones;

namespace API.Middleware
{
    public class ManejoExcepcionesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ManejoExcepcionesMiddleware> _logger;

        public ManejoExcepcionesMiddleware(RequestDelegate next, ILogger<ManejoExcepcionesMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NoEncontradoException ex)
            {
                await Responder(context, StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message);
            }
            catch (ReglaNegocioException ex)
            {
                await Responder(context, StatusCodes.Status409Conflict, "Regla de negocio incumplida", ex.Message);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // El cliente canceló la petición; no hay a quién responder.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
                await Responder(context, StatusCodes.Status500InternalServerError,
                    "Error interno del servidor", "Ocurrió un error inesperado.");
            }
        }

        private static Task Responder(HttpContext context, int estado, string titulo, string detalle)
        {
            return Results.Problem(detail: detalle, statusCode: estado, title: titulo).ExecuteAsync(context);
        }
    }
}
