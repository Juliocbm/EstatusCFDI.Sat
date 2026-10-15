using EstatusCFDI.Sat.Tests.Helpers;

namespace EstatusCFDI.Sat.Tests;

public class DatosConsultaCfdiTests
{
    private static readonly Guid Uuid = Guid.Parse("766049bd-0000-4000-8000-000000000001");

    [Fact]
    public void Constructor_RfcEnMinusculasYConEspacios_LoNormaliza()
    {
        var datos = new DatosConsultaCfdi(" eku9003173c9 ", "ñañ010101aa1", 10m, Uuid);

        Assert.Equal("EKU9003173C9", datos.RfcEmisor);
        Assert.Equal("ÑAÑ010101AA1", datos.RfcReceptor);
    }

    [Theory]
    [InlineData("XAXX010101000")]
    [InlineData("XEXX010101000")]
    [InlineData("A&B010101AB1")]
    [InlineData("URE180429TM6")]
    public void Constructor_RfcValido_Acepta(string rfc)
    {
        var datos = new DatosConsultaCfdi("EKU9003173C9", rfc, 10m, Uuid);

        Assert.Equal(rfc, datos.RfcReceptor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("EKU")]
    [InlineData("EKU900317")]
    [InlineData("EKU9013173C9")]
    [InlineData("EKU9003173C9X")]
    [InlineData("EKU900317-3C9")]
    public void Constructor_RfcInvalido_LanzaArgumentException(string rfc)
    {
        var ex = Assert.Throws<ArgumentException>(() => new DatosConsultaCfdi(rfc, "URE180429TM6", 10m, Uuid));

        Assert.Equal("rfcEmisor", ex.ParamName);
    }

    [Fact]
    public void Constructor_RfcNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DatosConsultaCfdi("EKU9003173C9", null!, 10m, Uuid));
    }

    [Fact]
    public void Constructor_TotalNegativo_LanzaArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DatosConsultaCfdi("EKU9003173C9", "URE180429TM6", -0.01m, Uuid));
    }

    [Fact]
    public void Constructor_UuidVacio_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new DatosConsultaCfdi("EKU9003173C9", "URE180429TM6", 10m, Guid.Empty));
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("abc&efgh")]
    [InlineData("abc efgh")]
    public void Constructor_FragmentoSelloInvalido_LanzaArgumentException(string fragmento)
    {
        Assert.Throws<ArgumentException>(() => new DatosConsultaCfdi("EKU9003173C9", "URE180429TM6", 10m, Uuid, fragmento));
    }

    [Theory]
    [InlineData("tZ9MSA==")]
    [InlineData("AbCd12+/")]
    public void Constructor_FragmentoSelloValido_LoConserva(string fragmento)
    {
        var datos = new DatosConsultaCfdi("EKU9003173C9", "URE180429TM6", 10m, Uuid, fragmento);

        Assert.Equal(fragmento, datos.FragmentoSello);
    }

    [Fact]
    public void ToString_SoloMuestraElUuid()
    {
        var texto = DatosPrueba.Datos().ToString();

        Assert.Contains(Uuid.ToString(), texto, StringComparison.Ordinal);
        Assert.DoesNotContain("EKU9003173C9", texto, StringComparison.Ordinal);
        Assert.DoesNotContain("1595", texto, StringComparison.Ordinal);
    }

    [Fact]
    public void Igualdad_MismosDatos_SonIguales()
    {
        Assert.Equal(DatosPrueba.Datos(), DatosPrueba.Datos());
        Assert.NotEqual(DatosPrueba.Datos(), DatosPrueba.Datos(fragmentoSello: null));
    }
}
