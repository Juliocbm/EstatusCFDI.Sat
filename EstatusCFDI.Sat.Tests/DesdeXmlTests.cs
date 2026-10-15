using EstatusCFDI.Sat.Tests.Helpers;

namespace EstatusCFDI.Sat.Tests;

public class DesdeXmlTests
{
    [Fact]
    public void DesdeXml_Cfdi40_LeeLosCincoDatos()
    {
        var datos = DatosConsultaCfdi.DesdeXml(DatosPrueba.Cfdi("cfdi40.xml"));

        Assert.Equal("EKU9003173C9", datos.RfcEmisor);
        Assert.Equal("URE180429TM6", datos.RfcReceptor);
        Assert.Equal(1595.00m, datos.Total);
        Assert.Equal(Guid.Parse("766049bd-0000-4000-8000-000000000001"), datos.Uuid);
        Assert.Equal("Q2QxMjM0", datos.FragmentoSello);
    }

    [Fact]
    public void DesdeXml_Cfdi33_LeeLosCincoDatos()
    {
        var datos = DatosConsultaCfdi.DesdeXml(DatosPrueba.Cfdi("cfdi33.xml"));

        Assert.Equal("XAXX010101000", datos.RfcReceptor);
        Assert.Equal(11558.1m, datos.Total);
        Assert.Equal(Guid.Parse("0A1B2C3D-4E5F-4A6B-8C7D-8E9F0A1B2C3D"), datos.Uuid);
        Assert.Equal("OTg3Ng==", datos.FragmentoSello);
    }

    [Fact]
    public void DesdeXml_RfcConAmpersand_LoDecodifica()
    {
        var datos = DatosConsultaCfdi.DesdeXml(DatosPrueba.Cfdi("cfdi40_rfc_ampersand.xml"));

        Assert.Equal("A&B010101AB1", datos.RfcReceptor);
    }

    [Fact]
    public void DesdeXml_ConBom_LoLee()
    {
        byte[] bom = [0xEF, 0xBB, 0xBF];
        var conBom = bom.Concat(DatosPrueba.Cfdi("cfdi40.xml")).ToArray();

        Assert.Equal("EKU9003173C9", DatosConsultaCfdi.DesdeXml(conBom).RfcEmisor);
    }

    [Fact]
    public void DesdeXml_Stream_NoLoCierra()
    {
        using var stream = new MemoryStream(DatosPrueba.Cfdi("cfdi40.xml"));

        DatosConsultaCfdi.DesdeXml(stream);

        Assert.True(stream.CanRead);
    }

    [Theory]
    [InlineData("cfdi40_sin_timbre.xml", "TimbreFiscalDigital")]
    [InlineData("cfdi40_sin_receptor.xml", "cfdi:Receptor/@Rfc")]
    [InlineData("cfdi40_total_invalido.xml", "@Total")]
    [InlineData("no_cfdi.xml", "cfdi:Comprobante")]
    public void DesdeXml_CfdiIncompleto_LanzaFormatException(string archivo, string datoFaltante)
    {
        var ex = Assert.Throws<FormatException>(() => DatosConsultaCfdi.DesdeXml(DatosPrueba.Cfdi(archivo)));

        Assert.Contains(datoFaltante, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DesdeXml_ConDtd_LaRechaza()
    {
        Assert.Throws<FormatException>(() => DatosConsultaCfdi.DesdeXml(DatosPrueba.Cfdi("cfdi40_con_dtd.xml")));
    }

    [Fact]
    public void DesdeXml_NoEsXml_LanzaFormatException()
    {
        Assert.Throws<FormatException>(() => DatosConsultaCfdi.DesdeXml("esto no es xml"u8.ToArray()));
    }

    [Fact]
    public void DesdeXml_Nulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => DatosConsultaCfdi.DesdeXml((byte[])null!));
        Assert.Throws<ArgumentNullException>(() => DatosConsultaCfdi.DesdeXml((Stream)null!));
    }
}
