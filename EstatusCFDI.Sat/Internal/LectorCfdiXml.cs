using System.Globalization;
using System.Xml.Linq;

namespace EstatusCFDI.Sat.Internal;

/// <summary>
/// Extrae de un CFDI 3.3 o 4.0 timbrado los datos que pide el SAT.
/// </summary>
internal static class LectorCfdiXml
{
    private static readonly XNamespace Cfdi40 = "http://www.sat.gob.mx/cfd/4";
    private static readonly XNamespace Cfdi33 = "http://www.sat.gob.mx/cfd/3";
    private static readonly XNamespace Timbre = "http://www.sat.gob.mx/TimbreFiscalDigital";

    public static DatosConsultaCfdi Leer(Stream xml)
    {
        ArgumentNullException.ThrowIfNull(xml);

        var comprobante = XmlSeguro.Cargar(xml, "El XML del CFDI").Root;
        if (comprobante is null
            || comprobante.Name.LocalName != "Comprobante"
            || (comprobante.Name.Namespace != Cfdi40 && comprobante.Name.Namespace != Cfdi33))
        {
            throw new FormatException("El XML no es un CFDI 3.3 o 4.0: falta el nodo cfdi:Comprobante.");
        }

        var cfdi = comprobante.Name.Namespace;
        var rfcEmisor = Atributo(comprobante.Element(cfdi + "Emisor"), "Rfc", "cfdi:Emisor/@Rfc");
        var rfcReceptor = Atributo(comprobante.Element(cfdi + "Receptor"), "Rfc", "cfdi:Receptor/@Rfc");
        var totalTexto = Atributo(comprobante, "Total", "cfdi:Comprobante/@Total");

        var timbre = comprobante.Elements(cfdi + "Complemento").Elements(Timbre + "TimbreFiscalDigital").FirstOrDefault()
            ?? throw new FormatException("El CFDI no está timbrado: falta tfd:TimbreFiscalDigital.");
        var uuidTexto = Atributo(timbre, "UUID", "tfd:TimbreFiscalDigital/@UUID");

        if (!decimal.TryParse(totalTexto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var total))
        {
            throw new FormatException($"cfdi:Comprobante/@Total no es un importe válido: '{totalTexto}'.");
        }

        if (!Guid.TryParse(uuidTexto, out var uuid))
        {
            throw new FormatException($"tfd:TimbreFiscalDigital/@UUID no es un UUID válido: '{uuidTexto}'.");
        }

        // El sello es opcional para el SAT: si falta, se consulta sin "fe".
        var sello = comprobante.Attribute("Sello")?.Value;
        var fragmentoSello = sello is { Length: >= 8 } ? sello[^8..] : null;

        try
        {
            return new DatosConsultaCfdi(rfcEmisor, rfcReceptor, total, uuid, fragmentoSello);
        }
        catch (ArgumentException ex)
        {
            throw new FormatException($"El CFDI trae un dato inválido: {ex.Message}", ex);
        }
    }

    private static string Atributo(XElement? elemento, string nombre, string ruta)
    {
        var valor = elemento?.Attribute(nombre)?.Value;
        return string.IsNullOrWhiteSpace(valor)
            ? throw new FormatException($"Al CFDI le falta {ruta}.")
            : valor;
    }
}
