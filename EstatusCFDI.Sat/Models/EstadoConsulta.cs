namespace EstatusCFDI.Sat;

/// <summary>
/// Resultado de consultar un CFDI en el servicio del SAT.
/// Solo <see cref="Vigente"/> y <see cref="Cancelado"/> son respuestas concluyentes del SAT.
/// </summary>
/// <remarks>
/// Los valores empiezan en 1 a propósito: <c>default(EstadoConsulta)</c> no equivale a <see cref="Vigente"/>.
/// </remarks>
public enum EstadoConsulta
{
    /// <summary>El SAT confirma que el CFDI existe y está vigente.</summary>
    Vigente = 1,

    /// <summary>El SAT confirma que el CFDI fue cancelado.</summary>
    Cancelado = 2,

    /// <summary>
    /// El SAT no tiene un CFDI con ese UUID (código 602). Justo después de timbrar, un CFDI puede tardar unos minutos en aparecer.
    /// </summary>
    NoEncontrado = 3,

    /// <summary>
    /// El SAT rechazó la expresión (código 601): falta un dato o alguno no coincide con el CFDI (por ejemplo, el total).
    /// </summary>
    ExpresionInvalida = 4,

    /// <summary>
    /// No hubo respuesta utilizable del SAT: tiempo de espera agotado, error de red, HTTP distinto de 200 o SOAP Fault.
    /// </summary>
    SinRespuesta = 5,

    /// <summary>El SAT respondió algo que la librería no sabe interpretar.</summary>
    RespuestaNoReconocida = 6,
}
