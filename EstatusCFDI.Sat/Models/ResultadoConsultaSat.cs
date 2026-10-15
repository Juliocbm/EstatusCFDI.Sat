namespace EstatusCFDI.Sat;

/// <summary>
/// Resultado de una consulta al SAT. Nunca se lanza una excepción por lo que el SAT responda o deje de responder:
/// todo llega aquí.
/// </summary>
/// <param name="Estado">Interpretación de la respuesta.</param>
/// <param name="Acuse">Campos crudos del SAT; <see langword="null"/> cuando no hubo respuesta interpretable.</param>
/// <param name="Detalle">Motivo técnico cuando <paramref name="Estado"/> es <see cref="EstadoConsulta.SinRespuesta"/> o <see cref="EstadoConsulta.RespuestaNoReconocida"/>.</param>
public sealed record ResultadoConsultaSat(EstadoConsulta Estado, AcuseSat? Acuse, string? Detalle)
{
    /// <summary>
    /// <see langword="true"/> cuando el SAT dio una respuesta definitiva (<see cref="EstadoConsulta.Vigente"/> o
    /// <see cref="EstadoConsulta.Cancelado"/>); en cualquier otro caso conviene reintentar más tarde.
    /// </summary>
    public bool EsConcluyente => Estado is EstadoConsulta.Vigente or EstadoConsulta.Cancelado;
}
