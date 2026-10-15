namespace EstatusCFDI.Sat;

/// <summary>
/// Campos de la respuesta del SAT, tal cual los devuelve el servicio. Sirven para diagnóstico y auditoría;
/// la decisión ya viene interpretada en <see cref="ResultadoConsultaSat.Estado"/>.
/// </summary>
/// <param name="CodigoEstatus">Por ejemplo, "S - Comprobante obtenido satisfactoriamente." o "N - 602: Comprobante no encontrado.".</param>
/// <param name="Estado">"Vigente", "Cancelado" o "No Encontrado".</param>
/// <param name="EsCancelable">Por ejemplo, "Cancelable sin aceptación". <see langword="null"/> si el SAT lo devolvió vacío.</param>
/// <param name="EstatusCancelacion">Estado de una solicitud de cancelación (por ejemplo, "Plazo vencido"). <see langword="null"/> si no hay.</param>
/// <param name="ValidacionEfos">"200" cuando el emisor no está en la lista de EFOS. <see langword="null"/> si el SAT lo devolvió vacío.</param>
public sealed record AcuseSat(
    string CodigoEstatus,
    string Estado,
    string? EsCancelable,
    string? EstatusCancelacion,
    string? ValidacionEfos);
