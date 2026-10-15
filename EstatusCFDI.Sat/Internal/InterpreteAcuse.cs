using System.Text.RegularExpressions;

namespace EstatusCFDI.Sat.Internal;

/// <summary>
/// Traduce el acuse del SAT a <see cref="EstadoConsulta"/>. Decide solo con <c>CodigoEstatus</c> y <c>Estado</c>:
/// <c>EsCancelable</c> y <c>EstatusCancelacion</c> llegan llenos incluso cuando el SAT rechaza la expresión (601).
/// </summary>
internal static partial class InterpreteAcuse
{
    public static EstadoConsulta Interpretar(AcuseSat acuse)
    {
        var codigo = PatronCodigo().Match(acuse.CodigoEstatus);
        if (!codigo.Success)
        {
            return EstadoConsulta.RespuestaNoReconocida;
        }

        if (string.Equals(codigo.Groups["tipo"].Value, "S", StringComparison.OrdinalIgnoreCase))
        {
            var estado = acuse.Estado.Trim();
            if (string.Equals(estado, "Vigente", StringComparison.OrdinalIgnoreCase))
            {
                return EstadoConsulta.Vigente;
            }

            if (string.Equals(estado, "Cancelado", StringComparison.OrdinalIgnoreCase))
            {
                return EstadoConsulta.Cancelado;
            }

            return string.Equals(estado, "No Encontrado", StringComparison.OrdinalIgnoreCase)
                ? EstadoConsulta.NoEncontrado
                : EstadoConsulta.RespuestaNoReconocida;
        }

        return codigo.Groups["numero"].Value switch
        {
            "602" => EstadoConsulta.NoEncontrado,
            "601" => EstadoConsulta.ExpresionInvalida,
            _ => EstadoConsulta.RespuestaNoReconocida,
        };
    }

    // "S - Comprobante obtenido satisfactoriamente." / "N - 602: Comprobante no encontrado."
    [GeneratedRegex(@"^\s*(?<tipo>[SN])\b(?:\s*-\s*(?<numero>\d{3}))?", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex PatronCodigo();
}
