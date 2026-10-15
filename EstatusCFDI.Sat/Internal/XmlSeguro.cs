using System.Xml;
using System.Xml.Linq;

namespace EstatusCFDI.Sat.Internal;

/// <summary>
/// Carga XML sin DTD ni resolución de entidades externas (evita XXE y la expansión de entidades),
/// tanto para el CFDI del consumidor como para la respuesta del SAT.
/// </summary>
internal static class XmlSeguro
{
    private static readonly XmlReaderSettings Configuracion = new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        CloseInput = false,
    };

    /// <exception cref="FormatException">El contenido no es XML válido o trae una DTD.</exception>
    public static XDocument Cargar(Stream contenido, string descripcion)
    {
        try
        {
            using var lector = XmlReader.Create(contenido, Configuracion);
            return XDocument.Load(lector);
        }
        catch (XmlException ex)
        {
            throw new FormatException($"{descripcion} no es XML válido: {ex.Message}", ex);
        }
    }
}
