using API.Validaciones.Usuario;
using Biblioteca.DTO.Usuario;

namespace API.Pruebas.Validaciones;

public class ConductorActualizarDtoValidadorPruebas
{
    private readonly ConductorActualizarDtoValidador _validador = new();

    private List<string> Errores(
        string nombre = "Carlos",
        string apellido = "Gómez",
        string correo = "carlos@sondrive.com",
        string celular = "3001234567",
        string? foto = null) =>
        _validador.Validate(new ConductorActualizarDto
        {
            Nombre = nombre,
            Apellido = apellido,
            Correo = correo,
            Celular = celular,
            Foto = foto
        }).Errors.Select(e => e.ErrorMessage).ToList();

    [Fact]
    public void Validar_NombreVacio_InformaNombreObligatorio() =>
        Assert.Contains("El nombre es obligatorio.", Errores(nombre: ""));

    [Fact]
    public void Validar_NombreDe101Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("El nombre no puede superar los 100 caracteres.", Errores(nombre: new string('a', 101)));

    [Fact]
    public void Validar_ApellidoVacio_InformaApellidoObligatorio() =>
        Assert.Contains("El apellido es obligatorio.", Errores(apellido: ""));

    [Fact]
    public void Validar_ApellidoDe101Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("El apellido no puede superar los 100 caracteres.", Errores(apellido: new string('a', 101)));

    [Fact]
    public void Validar_CorreoVacio_InformaCorreoObligatorio() =>
        Assert.Contains("El correo es obligatorio.", Errores(correo: ""));

    [Fact]
    public void Validar_CorreoConFormatoInvalido_InformaFormatoInvalido() =>
        Assert.Contains("El correo no tiene un formato válido.", Errores(correo: "no-es-un-correo"));

    [Fact]
    public void Validar_CorreoDe151Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("El correo no puede superar los 150 caracteres.",
            Errores(correo: new string('a', 139) + "@sondrive.com"));

    [Fact]
    public void Validar_CelularVacio_InformaCelularObligatorio() =>
        Assert.Contains("El celular es obligatorio.", Errores(celular: ""));

    [Theory]
    [InlineData("123")]
    [InlineData("2001234567")]
    [InlineData("30012345678")]
    public void Validar_CelularConFormatoInvalido_InformaFormatoDelCelular(string celular) =>
        Assert.Contains("El celular debe tener 10 dígitos y empezar por 3.", Errores(celular: celular));

    [Theory]
    [InlineData("abc")]
    [InlineData("ftp://servidor/foto.png")]
    [InlineData("/fotos/carlos.png")]
    public void Validar_FotoQueNoEsDireccionWeb_InformaDireccionInvalida(string foto) =>
        Assert.Contains("La foto debe ser una dirección web válida.", Errores(foto: foto));

    [Fact]
    public void Validar_FotoDeMasDe500Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("La dirección de la foto no puede superar los 500 caracteres.",
            Errores(foto: "https://ejemplo.com/" + new string('a', 500)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://ejemplo.com/foto.png")]
    [InlineData("http://ejemplo.com/foto.png")]
    public void Validar_FotoVaciaODireccionWeb_SinErrores(string? foto) =>
        Assert.Empty(Errores(foto: foto));

    [Fact]
    public void Validar_DatosValidos_SinErrores() =>
        Assert.Empty(Errores());
}
