namespace Biblioteca.Excepciones
{
    public class NoEncontradoException : Exception
    {
        public NoEncontradoException(string mensaje) : base(mensaje) { }
    }

    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
    }

    // Correo o clave incorrectos, o usuario desactivado (→ 401)
    public class CredencialesInvalidasException : Exception
    {
        public CredencialesInvalidasException(string mensaje) : base(mensaje) { }
    }
}
