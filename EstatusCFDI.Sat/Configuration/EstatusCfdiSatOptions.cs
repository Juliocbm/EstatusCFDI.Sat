namespace EstatusCFDI.Sat;

/// <summary>
/// Configuración de la consulta al SAT.
/// </summary>
public sealed class EstatusCfdiSatOptions
{
    /// <summary>URL oficial del servicio ConsultaCFDIService del SAT.</summary>
    public const string EndpointSat = "https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc";

    /// <summary>
    /// URL del servicio. Solo se acepta <c>https</c>. Por defecto, <see cref="EndpointSat"/>.
    /// </summary>
    public Uri Endpoint { get; set; } = new(EndpointSat);

    /// <summary>
    /// Tiempo máximo de espera por consulta. Al vencerse, el resultado es <see cref="EstadoConsulta.SinRespuesta"/>
    /// (no se lanza excepción). Por defecto, 25 segundos.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(25);
}
