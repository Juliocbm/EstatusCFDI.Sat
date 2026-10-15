using System.Net;

namespace EstatusCFDI.Sat.Tests.Helpers;

/// <summary>
/// Sustituye la red: responde lo que indique la prueba y guarda la última solicitud (con su cuerpo ya leído).
/// </summary>
internal sealed class ManejadorHttpFalso : HttpMessageHandler
{
    private readonly Func<CancellationToken, Task<HttpResponseMessage>> _responder;

    public ManejadorHttpFalso(Func<CancellationToken, Task<HttpResponseMessage>> responder) => _responder = responder;

    public HttpRequestMessage? Solicitud { get; private set; }

    public string? CuerpoSolicitud { get; private set; }

    public static ManejadorHttpFalso Responde(HttpStatusCode codigo, byte[] cuerpo) =>
        new(_ => Task.FromResult(new HttpResponseMessage(codigo) { Content = new ByteArrayContent(cuerpo) }));

    public static ManejadorHttpFalso RespondeSat(string archivo) =>
        Responde(HttpStatusCode.OK, DatosPrueba.RespuestaSat(archivo));

    /// <summary>No responde nunca: solo termina cuando se cancela el token.</summary>
    public static ManejadorHttpFalso NoResponde() =>
        new(async token =>
        {
            await Task.Delay(Timeout.Infinite, token);
            throw new InvalidOperationException("Inalcanzable.");
        });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Solicitud = request;
        CuerpoSolicitud = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return await _responder(cancellationToken);
    }
}
