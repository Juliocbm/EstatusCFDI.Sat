using System.Text.RegularExpressions;
using EstatusCFDI.Sat.Internal;

namespace EstatusCFDI.Sat;

/// <summary>
/// Datos con los que el SAT identifica un CFDI. Se validan y normalizan al construirse, así que una instancia siempre es consultable.
/// </summary>
public sealed partial record DatosConsultaCfdi
{
    /// <summary>
    /// Crea los datos de consulta.
    /// </summary>
    /// <param name="rfcEmisor">RFC del emisor (<c>cfdi:Emisor/@Rfc</c>).</param>
    /// <param name="rfcReceptor">RFC del receptor (<c>cfdi:Receptor/@Rfc</c>).</param>
    /// <param name="total">Total del comprobante (<c>cfdi:Comprobante/@Total</c>).</param>
    /// <param name="uuid">Folio fiscal (<c>tfd:TimbreFiscalDigital/@UUID</c>).</param>
    /// <param name="fragmentoSello">
    /// Últimos 8 caracteres de <c>cfdi:Comprobante/@Sello</c>. El SAT no lo exige, pero forma parte de la expresión oficial del QR.
    /// </param>
    /// <exception cref="ArgumentException">Algún dato no tiene un formato válido.</exception>
    public DatosConsultaCfdi(string rfcEmisor, string rfcReceptor, decimal total, Guid uuid, string? fragmentoSello = null)
    {
        RfcEmisor = NormalizarRfc(rfcEmisor, nameof(rfcEmisor));
        RfcReceptor = NormalizarRfc(rfcReceptor, nameof(rfcReceptor));

        if (total < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(total), total, "El total no puede ser negativo.");
        }

        if (uuid == Guid.Empty)
        {
            throw new ArgumentException("El UUID no puede ser vacío.", nameof(uuid));
        }

        if (fragmentoSello is not null && !PatronFragmentoSello().IsMatch(fragmentoSello))
        {
            throw new ArgumentException(
                "El fragmento del sello deben ser los últimos 8 caracteres (Base64) del atributo Sello.", nameof(fragmentoSello));
        }

        Total = total;
        Uuid = uuid;
        FragmentoSello = fragmentoSello;
    }

    /// <summary>RFC del emisor, en mayúsculas.</summary>
    public string RfcEmisor { get; }

    /// <summary>RFC del receptor, en mayúsculas.</summary>
    public string RfcReceptor { get; }

    /// <summary>Total del comprobante.</summary>
    public decimal Total { get; }

    /// <summary>Folio fiscal (UUID).</summary>
    public Guid Uuid { get; }

    /// <summary>Últimos 8 caracteres del sello del emisor, o <see langword="null"/> si no se proporcionó.</summary>
    public string? FragmentoSello { get; }

    /// <summary>
    /// Lee los datos de consulta de un CFDI 3.3 o 4.0 timbrado.
    /// </summary>
    /// <param name="xml">El XML del CFDI. No se cierra.</param>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> es <see langword="null"/>.</exception>
    /// <exception cref="FormatException">El XML no es un CFDI timbrado válido o le falta un dato.</exception>
    public static DatosConsultaCfdi DesdeXml(Stream xml) => LectorCfdiXml.Leer(xml);

    /// <summary>
    /// Lee los datos de consulta de un CFDI 3.3 o 4.0 timbrado.
    /// </summary>
    /// <param name="xml">El XML del CFDI.</param>
    /// <exception cref="ArgumentNullException"><paramref name="xml"/> es <see langword="null"/>.</exception>
    /// <exception cref="FormatException">El XML no es un CFDI timbrado válido o le falta un dato.</exception>
    public static DatosConsultaCfdi DesdeXml(byte[] xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        using var stream = new MemoryStream(xml, writable: false);
        return DesdeXml(stream);
    }

    /// <summary>
    /// Solo muestra el UUID, para que los RFC y el total no terminen en un log por accidente.
    /// </summary>
    public override string ToString() => $"CFDI {Uuid}";

    private static string NormalizarRfc(string rfc, string nombreParametro)
    {
        ArgumentNullException.ThrowIfNull(rfc, nombreParametro);

        var normalizado = rfc.Trim().ToUpperInvariant();
        if (!PatronRfc().IsMatch(normalizado))
        {
            throw new ArgumentException($"'{rfc}' no es un RFC válido.", nombreParametro);
        }

        return normalizado;
    }

    // Patrón del tipo t_RFC del esquema oficial del CFDI (tdCFDI.xsd).
    [GeneratedRegex("^[A-Z&Ñ]{3,4}[0-9]{2}(0[1-9]|1[012])(0[1-9]|[12][0-9]|3[01])[A-Z0-9]{2}[0-9A]$", RegexOptions.CultureInvariant)]
    private static partial Regex PatronRfc();

    [GeneratedRegex("^[A-Za-z0-9+/=]{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex PatronFragmentoSello();
}
