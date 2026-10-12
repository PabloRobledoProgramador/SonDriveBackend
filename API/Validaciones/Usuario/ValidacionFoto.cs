namespace API.Validaciones.Usuario
{
    // Regla de la foto compartida por el alta de usuarios y la edición de conductores,
    // para que ambos validadores no puedan divergir.
    public static class ValidacionFoto
    {
        public const int LongitudMaxima = 500; // longitud de la columna Usuarios.Foto
        public const string MensajeDireccionInvalida = "La foto debe ser una dirección web válida.";
        public const string MensajeLongitudMaxima = "La dirección de la foto no puede superar los 500 caracteres.";

        public static bool EsDireccionWebValida(string? foto) =>
            Uri.TryCreate(foto?.Trim(), UriKind.Absolute, out var direccion)
            && (direccion.Scheme == Uri.UriSchemeHttp || direccion.Scheme == Uri.UriSchemeHttps);
    }
}
