using System.Globalization;
using EstatusCFDI.Sat.Internal;
using EstatusCFDI.Sat.Tests.Helpers;

namespace EstatusCFDI.Sat.Tests;

public class ExpresionImpresaTests
{
    [Fact]
    public void Construir_ConFragmento_ArmaLaExpresionCompleta()
    {
        var expresion = ExpresionImpresa.Construir(DatosPrueba.Datos());

        Assert.Equal("?re=EKU9003173C9&rr=URE180429TM6&tt=1595.00&id=766049BD-0000-4000-8000-000000000001&fe=Q2QxMjM0", expresion);
    }

    [Fact]
    public void Construir_SinFragmento_OmiteFe()
    {
        var expresion = ExpresionImpresa.Construir(DatosPrueba.Datos(fragmentoSello: null));

        Assert.Equal("?re=EKU9003173C9&rr=URE180429TM6&tt=1595.00&id=766049BD-0000-4000-8000-000000000001", expresion);
    }

    [Fact]
    public void Construir_RfcConAmpersand_LoEscribeComoEntidad()
    {
        var expresion = ExpresionImpresa.Construir(DatosPrueba.Datos(rfcReceptor: "A&B010101AB1"));

        Assert.Contains("&rr=A&amp;B010101AB1&tt=", expresion, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("16799.99", "16799.99")]
    [InlineData("11558.1", "11558.10")]
    [InlineData("1595", "1595.00")]
    [InlineData("0", "0.00")]
    [InlineData("0.5", "0.50")]
    [InlineData("1.234567", "1.234567")]
    [InlineData("1.2345678", "1.234568")]
    [InlineData("1234567.89", "1234567.89")]
    public void FormatearTotal_UsaFormaCanonica(string total, string esperado)
    {
        Assert.Equal(esperado, ExpresionImpresa.FormatearTotal(decimal.Parse(total, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void FormatearTotal_NoDependeDeLaCulturaActual()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");

            Assert.Equal("16799.99", ExpresionImpresa.FormatearTotal(16799.99m));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
