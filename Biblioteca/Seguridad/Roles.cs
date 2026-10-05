namespace Biblioteca.Seguridad
{
    // Roles válidos de la aplicación. Usar estas constantes en lugar de escribir el texto a mano.
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Conductor = "Conductor";

        public static readonly IReadOnlyList<string> Todos = new[] { Administrador, Conductor };
    }
}
