using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using EstatusCFDI.Sat.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EstatusCFDI.Sat;

/// <summary>
/// Cliente del servicio ConsultaCFDIService del SAT.
/// </summary>
/// <remarks>
/// Con DI se registra con <c>services.AddEstatusCfdiSat()</c>. Sin DI se construye directo; el <see cref="HttpClient"/>
/// lo administra quien lo crea y conviene reutilizarlo entre consultas.
/// </remarks>
public sealed partial class ConsultaEstatusCfdiSat : IConsultaEstatusCfdi
{
    private readonly HttpClient _httpClient;
    private readonly EstatusCfdiSatOptions _opciones;
    private readonly ILogger<ConsultaEstatusCfdiSat> _logger;

    /// <summary>
    /// Crea el cliente.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP. Su propio <see cref="HttpClient.Timeout"/> debe ser mayor que <see cref="EstatusCfdiSatOptions.Timeout"/>.</param>
    /// <param name="opciones">Configuración; se valida al construir.</param>
    /// <param name="logger">Logger. Solo registra el UUID y el resultado, nunca RFC ni total.</param>
    /// <exception cref="ArgumentException">La configuración no es válida.</exception>
    public ConsultaEstatusCfdiSat(HttpClient httpClient, IOptions<EstatusCfdiSatOptions> opciones, ILogger<ConsultaEstatusCfdiSat> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(opciones);
        ArgumentNullException.ThrowIfNull(logger);

        var valor = opciones.Value;
        var errores = ValidacionEstatusCfdiSatOptions.Errores(valor);
        if (errores.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errores), nameof(opciones));
        }

        _httpClient = httpClient;
        _opciones = valor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ResultadoConsultaSat> ConsultarAsync(DatosConsultaCfdi datos, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var inicio = Stopwatch.GetTimestamp();
        var resultado = await EnviarAsync(datos, cancellationToken).ConfigureAwait(false);
        var milisegundos = (long)Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;

        if (resultado.Estado is EstadoConsulta.SinRespuesta or EstadoConsulta.RespuestaNoReconocida)
        {
            LogNoConfirmado(datos.Uuid, resultado.Estado, milisegundos, resultado.Detalle);
        }
        else
        {
            LogConsulta(datos.Uuid, resultado.Estado, milisegundos);
        }

        return resultado;
    }

    private async Task<ResultadoConsultaSat> EnviarAsync(DatosConsultaCfdi datos, CancellationToken cancellationToken)
    {
        using var limite = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limite.CancelAfter(_opciones.Timeout);

        try
        {
            using var solicitud = CrearSolicitud(datos);
            using var respuesta = await _httpClient
                .SendAsync(solicitud, HttpCompletionOption.ResponseContentRead, limite.Token)
                .ConfigureAwait(false);
            var contenido = await respuesta.Content.ReadAsStreamAsync(limite.Token).ConfigureAwait(false);
            await using (contenido.ConfigureAwait(false))
            {
                if (!respuesta.IsSuccessStatusCode)
                {
                    var falla = SobreSoap.LeerFalla(contenido);
                    var detalle = string.Create(
                        CultureInfo.InvariantCulture, $"El SAT respondió HTTP {(int)respuesta.StatusCode} {respuesta.ReasonPhrase}.");
                    return SinRespuesta(falla is null ? detalle : $"{detalle} {falla}");
                }

                try
                {
                    var acuse = SobreSoap.LeerRespuesta(contenido);
                    return new ResultadoConsultaSat(InterpreteAcuse.Interpretar(acuse), acuse, null);
                }
                catch (FormatException ex)
                {
                    return new ResultadoConsultaSat(EstadoConsulta.RespuestaNoReconocida, null, ex.Message);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            // Venció el tiempo de espera propio (o el del HttpClient); no es una cancelación del llamador.
            return SinRespuesta(string.Create(
                CultureInfo.InvariantCulture, $"Tiempo de espera agotado ({_opciones.Timeout.TotalSeconds:0.###} s)."));
        }
        catch (HttpRequestException ex)
        {
            return SinRespuesta($"Error de comunicación con el SAT: {ex.Message}");
        }
    }

    private HttpRequestMessage CrearSolicitud(DatosConsultaCfdi datos)
    {
        var contenido = new ByteArrayContent(SobreSoap.CrearSolicitud(ExpresionImpresa.Construir(datos)));
        contenido.Headers.ContentType = new MediaTypeHeaderValue("text/xml") { CharSet = "utf-8" };

        var solicitud = new HttpRequestMessage(HttpMethod.Post, _opciones.Endpoint) { Content = contenido };
        solicitud.Headers.Add("SOAPAction", $"\"{SobreSoap.AccionSoap}\"");
        return solicitud;
    }

    private static ResultadoConsultaSat SinRespuesta(string detalle) => new(EstadoConsulta.SinRespuesta, null, detalle);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "SAT: CFDI {Uuid} {Estado} ({Milisegundos} ms).")]
    private partial void LogConsulta(Guid uuid, EstadoConsulta estado, long milisegundos);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "SAT: no se pudo confirmar el CFDI {Uuid}: {Estado} ({Milisegundos} ms). {Detalle}")]
    private partial void LogNoConfirmado(Guid uuid, EstadoConsulta estado, long milisegundos, string? detalle);
}
