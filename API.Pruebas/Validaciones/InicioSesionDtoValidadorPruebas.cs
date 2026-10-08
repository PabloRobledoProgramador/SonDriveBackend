using API.Validaciones.InicioSesion;
using Biblioteca.DTO.InicioSesion;

namespace API.Pruebas.Validaciones;

public class InicioSesionDtoValidadorPruebas
{
    private readonly InicioSesionDtoValidador _validador = new();

    private List<string> Errores(string correo, string clave) =>
        _validador.Validate(new InicioSesionDTO { Correo = correo, Clave = clave })
            .Errors.Select(e => e.ErrorMessage).ToList();

    [Fact]
    public void Validar_CorreoVacio_InformaCorreoObligatorio() =>
        Assert.Contains("El correo es obligatorio.", Errores("", "Clave12345*"));

    [Fact]
    public void Validar_CorreoConFormatoInvalido_InformaFormatoInvalido() =>
        Assert.Contains("El correo no tiene un formato válido.", Errores("no-es-un-correo", "Clave12345*"));

    [Fact]
    public void Validar_CorreoDe151Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("El correo no puede superar los 150 caracteres.",
            Errores(new string('a', 139) + "@sondrive.com", "Clave12345*"));

    [Fact]
    public void Validar_ClaveVacia_InformaClaveObligatoria() =>
        Assert.Contains("La clave es obligatoria.", Errores("admin@sondrive.com", ""));

    [Fact]
    public void Validar_ClaveDe73Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("La clave no puede superar los 72 caracteres.", Errores("admin@sondrive.com", new string('x', 73)));

    [Fact]
    public void Validar_DatosValidos_SinErrores() =>
        Assert.Empty(Errores("admin@sondrive.com", "Admin12345*"));
}
