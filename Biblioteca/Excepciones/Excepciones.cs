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
}