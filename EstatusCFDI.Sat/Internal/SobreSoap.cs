using System.Text;
using System.Xml.Linq;

namespace EstatusCFDI.Sat.Internal;

/// <summary>
/// Arma la solicitud SOAP 1.1 de la operación <c>Consulta</c> del SAT y lee su respuesta.
/// </summary>
internal static class SobreSoap
{
    public const string AccionSoap = "http://tempuri.org/IConsultaCFDIService/Consulta";

    private static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace Servicio = "http://tempuri.org/";
    private static readonly XNamespace Contrato = "http://schemas.datacontract.org/2004/07/Sat.Cfdi.Negocio.ConsultaCfdi.Servicio";

    /// <summary>
    /// La expresión va como texto del elemento: el serializador la escapa (equivale al CDATA que muestra la documentación del SAT).
    /// </summary>
    public static byte[] CrearSolicitud(string expresionImpresa)
    {
        var sobre = new XElement(
            Soap + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", Soap),
            new XAttribute(XNamespace.Xmlns + "tem", Servicio),
            new XElement(
                Soap + "Body",
                new XElement(Servicio + "Consulta", new XElement(Servicio + "expresionImpresa", expresionImpresa))));

        return Encoding.UTF8.GetBytes(sobre.ToString(SaveOptions.DisableFormatting));
    }

    /// <exception cref="FormatException">La respuesta no es XML o no trae <c>ConsultaResult</c> con su código y estado.</exception>
    public static AcuseSat LeerRespuesta(Stream contenido)
    {
        var resultado = XmlSeguro.Cargar(contenido, "La respuesta del SAT").Descendants(Servicio + "ConsultaResult").FirstOrDefault()
            ?? throw new FormatException("La respuesta del SAT no trae ConsultaResult.");

        string? Campo(string nombre) => resultado.Element(Contrato + nombre)?.Value;

        var codigoEstatus = Campo("CodigoEstatus");
        var estado = Campo("Estado");
        if (codigoEstatus is null || estado is null)
        {
            throw new FormatException("La respuesta del SAT no trae CodigoEstatus o Estado.");
        }

        return new AcuseSat(
            codigoEstatus,
            estado,
            NuloSiVacio(Campo("EsCancelable")),
            NuloSiVacio(Campo("EstatusCancelacion")),
            NuloSiVacio(Campo("ValidacionEFOS")));
    }

    /// <summary>
    /// Mensaje de un SOAP Fault, si el cuerpo lo trae; <see langword="null"/> si no se puede leer.
    /// </summary>
    public static string? LeerFalla(Stream contenido)
    {
        try
        {
            return NuloSiVacio(XmlSeguro.Cargar(contenido, "La falla del SAT").Descendants("faultstring").FirstOrDefault()?.Value);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static string? NuloSiVacio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor;
}
