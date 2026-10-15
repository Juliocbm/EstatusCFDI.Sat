namespace EstatusCFDI.Sat;

/// <summary>
/// Consulta el estado de un CFDI en el servicio del SAT.
/// </summary>
public interface IConsultaEstatusCfdi
{
    /// <summary>
    /// Consulta si el CFDI está vigente o cancelado.
    /// </summary>
    /// <param name="datos">Datos del CFDI; se pueden leer del XML timbrado con <see cref="DatosConsultaCfdi.DesdeXml(Stream)"/>.</param>
    /// <param name="cancellationToken">Cancela la consulta. El tiempo de espera configurado no cuenta como cancelación.</param>
    /// <returns>El resultado; nunca lanza excepción por la respuesta (o falta de respuesta) del SAT.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="datos"/> es <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> se canceló.</exception>
    Task<ResultadoConsultaSat> ConsultarAsync(DatosConsultaCfdi datos, CancellationToken cancellationToken = default);
}
