using API.Validaciones.Usuario;
using Biblioteca.DTO.Usuario;

namespace API.Pruebas.Validaciones;

// Cubre la regla de la foto, que el alta comparte con la edición de conductores (ValidacionFoto)
public class UsuarioCrearDtoValidadorPruebas
{
    private readonly UsuarioCrearDtoValidador _validador = new();

    private List<string> Errores(string? foto) =>
        _validador.Validate(new UsuarioCrearDto
        {
            Nombre = "Ana",
            Apellido = "Torres",
            Correo = "ana@sondrive.com",
            Clave = "Conductor123*",
            Celular = "3012345678",
            Foto = foto
        }).Errors.Select(e => e.ErrorMessage).ToList();

    [Theory]
    [InlineData("abc")]
    [InlineData("ftp://servidor/foto.png")]
    public void Validar_FotoQueNoEsDireccionWeb_InformaDireccionInvalida(string foto) =>
        Assert.Contains("La foto debe ser una dirección web válida.", Errores(foto));

    [Fact]
    public void Validar_FotoDeMasDe500Caracteres_InformaLongitudMaxima() =>
        Assert.Contains("La dirección de la foto no puede superar los 500 caracteres.",
            Errores("https://ejemplo.com/" + new string('a', 500)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://ejemplo.com/foto.png")]
    public void Validar_DatosValidosConFotoVaciaODireccionWeb_SinErrores(string? foto) =>
        Assert.Empty(Errores(foto));
}
