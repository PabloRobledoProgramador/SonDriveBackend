using API.Validaciones.Vehiculo;
using Biblioteca.DTO.Vehiculo;

namespace API.Pruebas.Validaciones;

public class VehiculoDatosDtoValidatorPruebas
{
    private readonly VehiculoDatosDtoValidator _validador = new();

    private static VehiculoDatosDTO DatosValidos() => new()
    {
        IdUsuario = 2,
        Placa = "ABC123",
        Modelo = "Kia Rio",
        AnnoModelo = "2022",
        Tipo = "Sedan",
        Kilometraje = 48200,
        FechaUltimoServicio = DateTime.Today.AddDays(-30),
        VencimientoSoat = DateTime.Today.AddDays(200),
        VencimientoTecnomecanica = DateTime.Today.AddDays(300),
        VencimientoLicencia = DateTime.Today.AddDays(400)
    };

    private List<string> Errores(Action<VehiculoDatosDTO>? cambiar = null)
    {
        var dto = DatosValidos();
        cambiar?.Invoke(dto);
        return _validador.Validate(dto).Errors.Select(e => e.ErrorMessage).ToList();
    }

    [Fact]
    public void Validar_DatosValidos_NoInformaErrores() =>
        Assert.Empty(Errores());

    [Fact]
    public void Validar_SinConductor_InformaConductorObligatorio() =>
        Assert.Contains("El conductor es obligatorio.", Errores(d => d.IdUsuario = 0));

    [Fact]
    public void Validar_PlacaVacia_InformaPlacaObligatoria() =>
        Assert.Contains("La placa es obligatoria.", Errores(d => d.Placa = ""));

    [Fact]
    public void Validar_PlacaConFormatoIncorrecto_InformaFormato() =>
        Assert.Contains(
            "La placa debe tener 3 letras y 3 números, por ejemplo SXK 204.",
            Errores(d => d.Placa = "AB12"));

    [Theory]
    [InlineData("ABC123")]
    [InlineData("abc 123")]
    public void Validar_PlacaConFormatoAceptado_NoInformaErrores(string placa) =>
        Assert.Empty(Errores(d => d.Placa = placa));

    [Fact]
    public void Validar_ModeloVacio_InformaModeloObligatorio() =>
        Assert.Contains("El modelo es obligatorio.", Errores(d => d.Modelo = ""));

    [Fact]
    public void Validar_ModeloDe101Caracteres_InformaLongitudMaxima() =>
        Assert.Contains(
            "El modelo no puede superar los 100 caracteres.",
            Errores(d => d.Modelo = new string('a', 101)));

    [Fact]
    public void Validar_AnnoVacio_InformaAnnoObligatorio() =>
        Assert.Contains("El año del modelo es obligatorio.", Errores(d => d.AnnoModelo = ""));

    [Fact]
    public void Validar_AnnoDeDosDigitos_InformaCuatroDigitos() =>
        Assert.Contains("El año debe tener 4 dígitos.", Errores(d => d.AnnoModelo = "22"));

    [Fact]
    public void Validar_AnnoAnteriorA1990_InformaAnnoNoValido() =>
        Assert.Contains("El año del modelo no es válido.", Errores(d => d.AnnoModelo = "1989"));

    [Fact]
    public void Validar_AnnoDosAnnosAdelante_InformaAnnoNoValido() =>
        Assert.Contains(
            "El año del modelo no es válido.",
            Errores(d => d.AnnoModelo = (DateTime.Today.Year + 2).ToString()));

    [Fact]
    public void Validar_TipoVacio_InformaTipoObligatorio() =>
        Assert.Contains("El tipo de vehículo es obligatorio.", Errores(d => d.Tipo = ""));

    [Fact]
    public void Validar_TipoDe51Caracteres_InformaLongitudMaxima() =>
        Assert.Contains(
            "El tipo no puede superar los 50 caracteres.",
            Errores(d => d.Tipo = new string('a', 51)));

    [Fact]
    public void Validar_KilometrajeNegativo_InformaKilometraje() =>
        Assert.Contains("El kilometraje no puede ser negativo.", Errores(d => d.Kilometraje = -1));

    [Fact]
    public void Validar_UltimoServicioFuturo_InformaFechaFutura() =>
        Assert.Contains(
            "La fecha del último servicio no puede ser futura.",
            Errores(d => d.FechaUltimoServicio = DateTime.Today.AddDays(1)));

    [Fact]
    public void Validar_SinUltimoServicio_NoInformaErrores() =>
        Assert.Empty(Errores(d => d.FechaUltimoServicio = null));

    [Fact]
    public void Validar_SinVencimientoSoat_InformaSoatObligatorio() =>
        Assert.Contains("El vencimiento del SOAT es obligatorio.", Errores(d => d.VencimientoSoat = default));

    [Fact]
    public void Validar_SinVencimientoTecnomecanica_InformaTecnomecanicaObligatoria() =>
        Assert.Contains(
            "El vencimiento de la tecnomecánica es obligatorio.",
            Errores(d => d.VencimientoTecnomecanica = default));

    [Fact]
    public void Validar_SinVencimientoLicencia_InformaLicenciaObligatoria() =>
        Assert.Contains(
            "El vencimiento de la licencia es obligatorio.",
            Errores(d => d.VencimientoLicencia = default));

    [Fact]
    public void Validar_VencimientoDeAyer_NoInformaErrores() =>
        Assert.Empty(Errores(d => d.VencimientoSoat = DateTime.Today.AddDays(-1)));
}
