using System.Xml.Linq;
using EstatusCFDI.Sat.Internal;
using EstatusCFDI.Sat.Tests.Helpers;

namespace EstatusCFDI.Sat.Tests;

public class SobreSoapTests
{
    private static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace Servicio = "http://tempuri.org/";

    [Fact]
    public void CrearSolicitud_LlevaLaExpresionEnElNodoEsperado()
    {
        const string Expresion = "?re=EKU9003173C9&rr=A&amp;B010101AB1&tt=1.00&id=X";

        var sobre = XDocument.Parse(System.Text.Encoding.UTF8.GetString(SobreSoap.CrearSolicitud(Expresion)));

        var nodo = sobre.Root!.Element(Soap + "Body")!.Element(Servicio + "Consulta")!.Element(Servicio + "expresionImpresa")!;
        Assert.Equal(Expresion, nodo.Value);
    }

    [Theory]
    [InlineData("vigente.xml", "Vigente", "Cancelable sin aceptación", null)]
    [InlineData("cancelado.xml", "Cancelado", "Cancelable con aceptación", "Plazo vencido")]
    [InlineData("no_encontrado.xml", "No Encontrado", null, null)]
    public void LeerRespuesta_RespuestaReal_LeeLosCampos(string archivo, string estado, string? esCancelable, string? estatusCancelacion)
    {
        using var contenido = new MemoryStream(DatosPrueba.RespuestaSat(archivo));

        var acuse = SobreSoap.LeerRespuesta(contenido);

        Assert.Equal(estado, acuse.Estado);
        Assert.Equal(esCancelable, acuse.EsCancelable);
        Assert.Equal(estatusCancelacion, acuse.EstatusCancelacion);
    }

    [Fact]
    public void LeerRespuesta_DatoNoCoincide_TraeCamposLlenosConCodigo601()
    {
        using var contenido = new MemoryStream(DatosPrueba.RespuestaSat("dato_no_coincide.xml"));

        var acuse = SobreSoap.LeerRespuesta(contenido);

        Assert.StartsWith("N - 601", acuse.CodigoEstatus, StringComparison.Ordinal);
        Assert.NotNull(acuse.EsCancelable);
        Assert.Equal(EstadoConsulta.ExpresionInvalida, InterpreteAcuse.Interpretar(acuse));
    }

    [Theory]
    [InlineData("sin_resultado.xml")]
    [InlineData("con_dtd.xml")]
    [InlineData("fault.xml")]
    public void LeerRespuesta_SinResultadoUtil_LanzaFormatException(string archivo)
    {
        using var contenido = new MemoryStream(DatosPrueba.RespuestaSat(archivo));

        Assert.Throws<FormatException>(() => SobreSoap.LeerRespuesta(contenido));
    }

    [Fact]
    public void LeerFalla_Fault_DevuelveElMensaje()
    {
        using var contenido = new MemoryStream(DatosPrueba.RespuestaSat("fault.xml"));

        Assert.Equal("El servicio no está disponible.", SobreSoap.LeerFalla(contenido));
    }

    [Fact]
    public void LeerFalla_NoEsXml_DevuelveNulo()
    {
        using var contenido = new MemoryStream("<html>Error"u8.ToArray());

        Assert.Null(SobreSoap.LeerFalla(contenido));
    }
}
