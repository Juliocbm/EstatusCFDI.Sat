using System.Globalization;
using System.Text;

namespace EstatusCFDI.Sat.Internal;

/// <summary>
/// Arma la expresión impresa que recibe el SAT: <c>?re=…&amp;rr=…&amp;tt=…&amp;id=…&amp;fe=…</c>.
/// </summary>
internal static class ExpresionImpresa
{
    public static string Construir(DatosConsultaCfdi datos)
    {
        var expresion = new StringBuilder(128)
            .Append("?re=").Append(EscaparRfc(datos.RfcEmisor))
            .Append("&rr=").Append(EscaparRfc(datos.RfcReceptor))
            .Append("&tt=").Append(FormatearTotal(datos.Total))
            .Append("&id=").Append(datos.Uuid.ToString("D").ToUpperInvariant());

        if (datos.FragmentoSello is not null)
        {
            expresion.Append("&fe=").Append(datos.FragmentoSello);
        }

        return expresion.ToString();
    }

    /// <summary>
    /// El SAT compara el total por valor (acepta "11558.1", "11558.10" y "11558.100000"), así que se usa una forma
    /// canónica: punto decimal, sin separador de miles, de 2 a 6 decimales.
    /// </summary>
    internal static string FormatearTotal(decimal total) => total.ToString("0.00####", CultureInfo.InvariantCulture);

    /// <summary>
    /// El SAT separa los campos de la expresión con '&amp;', así que un RFC que lo contiene (por ejemplo, "SR&amp;061229V17")
    /// debe ir como la entidad "&amp;amp;" escrita literalmente. Verificado contra el SAT el 2026-10-15 con un CFDI real:
    /// con el '&amp;' crudo o con "%26" responde 601; con "&amp;amp;" responde el estado.
    /// </summary>
    internal static string EscaparRfc(string rfc) => rfc.Replace("&", "&amp;", StringComparison.Ordinal);
}
