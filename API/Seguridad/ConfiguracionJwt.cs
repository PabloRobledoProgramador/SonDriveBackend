namespace API.Seguridad
{
    // Se lee de la sección "Jwt" de la configuración
    public class ConfiguracionJwt
    {
        public const string Seccion = "Jwt";

        // Clave secreta para firmar los tokens (HMAC-SHA256): mínimo 32 caracteres.
        // En producción debe venir de variables de entorno o user secrets, nunca del repositorio.
        public string Clave { get; set; } = string.Empty;
        public string Emisor { get; set; } = string.Empty;
        public string Audiencia { get; set; } = string.Empty;
        public int MinutosExpiracion { get; set; } = 480;
    }
}
