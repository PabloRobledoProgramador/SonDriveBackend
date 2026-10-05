using Biblioteca.Modelo;

namespace Biblioteca.Servicios.Implementaciones
{
    public static class MapaPuestos
    {
        public const string CodigoConductor = "Conductor";
        public const string TipoCajuela = "Cajuela";

        public static readonly IReadOnlyList<(string Codigo, string Tipo)> Estandar = new List<(string, string)>
        {
            ("Conductor", "Ventana"),
            ("Copiloto",  "Ventana"),
            ("Izquierda", "Ventana"),
            ("Centro",    "Pasillo"),
            ("Derecha",   "Ventana"),
            ("C1",        TipoCajuela),
            ("C2",        TipoCajuela),
            ("C3",        TipoCajuela),
            ("C4",        TipoCajuela)
        };

        public static bool EsConductor(Puesto p) =>
            p.CodigoPuesto.Descripcion.Equals(CodigoConductor, StringComparison.OrdinalIgnoreCase);

        public static bool EsCajuela(Puesto p) =>
            p.TipoPuesto.Descripcion.Equals(TipoCajuela, StringComparison.OrdinalIgnoreCase);
    }
}