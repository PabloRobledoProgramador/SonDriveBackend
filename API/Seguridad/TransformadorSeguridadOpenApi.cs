using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace API.Seguridad
{
    // Agrega el esquema "Bearer" al documento OpenAPI para poder enviar el token desde Swagger UI (botón Authorize)
    public class TransformadorSeguridadOpenApi : IOpenApiDocumentTransformer
    {
        private const string Esquema = "Bearer";

        public Task TransformAsync(OpenApiDocument documento, OpenApiDocumentTransformerContext contexto, CancellationToken ct)
        {
            documento.Components ??= new OpenApiComponents();
            documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            documento.Components.SecuritySchemes[Esquema] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token obtenido en POST /api/autenticacion/iniciar-sesion"
            };

            documento.Security ??= new List<OpenApiSecurityRequirement>();
            documento.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(Esquema, documento)] = new List<string>()
            });

            return Task.CompletedTask;
        }
    }
}
