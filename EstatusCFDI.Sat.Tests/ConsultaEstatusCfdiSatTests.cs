using System.Net;
using System.Xml.Linq;
using EstatusCFDI.Sat.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EstatusCFDI.Sat.Tests;

public sealed class ConsultaEstatusCfdiSatTests : IDisposable
{
    private readonly List<HttpClient> _clientes = [];

    public void Dispose()
    {
        foreach (var cliente in _clientes)
        {
            cliente.Dispose();
        }
    }

    [Theory]
    [InlineData("vigente.xml", EstadoConsulta.Vigente)]
    [InlineData("cancelado.xml", EstadoConsulta.Cancelado)]
    [InlineData("no_encontrado.xml", EstadoConsulta.NoEncontrado)]
    [InlineData("expresion_invalida.xml", EstadoConsulta.ExpresionInvalida)]
    [InlineData("dato_no_coincide.xml", EstadoConsulta.ExpresionInvalida)]
    public async Task ConsultarAsync_RespuestaDelSat_LaInterpreta(string archivo, EstadoConsulta esperado)
    {
        var consulta = Crear(ManejadorHttpFalso.RespondeSat(archivo));

        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.Equal(esperado, resultado.Estado);
        Assert.NotNull(resultado.Acuse);
        Assert.Null(resultado.Detalle);
    }

    [Fact]
    public async Task ConsultarAsync_Vigente_EsConcluyente()
    {
        var resultado = await Crear(ManejadorHttpFalso.RespondeSat("vigente.xml")).ConsultarAsync(DatosPrueba.Datos());

        Assert.True(resultado.EsConcluyente);
    }

    [Fact]
    public async Task ConsultarAsync_ArmaLaSolicitudSoap()
    {
        var manejador = ManejadorHttpFalso.RespondeSat("vigente.xml");

        await Crear(manejador).ConsultarAsync(DatosPrueba.Datos(rfcReceptor: "A&B010101AB1"));

        var solicitud = manejador.Solicitud!;
        Assert.Equal(HttpMethod.Post, solicitud.Method);
        Assert.Equal(new Uri(EstatusCfdiSatOptions.EndpointSat), solicitud.RequestUri);
        Assert.Equal("\"http://tempuri.org/IConsultaCFDIService/Consulta\"", solicitud.Headers.GetValues("SOAPAction").Single());
        Assert.Equal("text/xml", solicitud.Content!.Headers.ContentType!.MediaType);
        Assert.Equal("utf-8", solicitud.Content.Headers.ContentType.CharSet);

        // El valor del nodo (ya sin el escapado XML) lleva la entidad "&amp;" literal, como la pide el SAT.
        XNamespace servicio = "http://tempuri.org/";
        var expresion = XDocument.Parse(manejador.CuerpoSolicitud!).Descendants(servicio + "expresionImpresa").Single().Value;
        Assert.Equal("?re=EKU9003173C9&rr=A&amp;B010101AB1&tt=1595.00&id=766049BD-0000-4000-8000-000000000001&fe=Q2QxMjM0", expresion);
    }

    [Fact]
    public async Task ConsultarAsync_Http500ConFault_DevuelveSinRespuestaConDetalle()
    {
        var consulta = Crear(ManejadorHttpFalso.Responde(HttpStatusCode.InternalServerError, DatosPrueba.RespuestaSat("fault.xml")));

        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.Equal(EstadoConsulta.SinRespuesta, resultado.Estado);
        Assert.Null(resultado.Acuse);
        Assert.Contains("HTTP 500", resultado.Detalle, StringComparison.Ordinal);
        Assert.Contains("El servicio no está disponible.", resultado.Detalle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConsultarAsync_Http503SinCuerpo_DevuelveSinRespuesta()
    {
        var consulta = Crear(ManejadorHttpFalso.Responde(HttpStatusCode.ServiceUnavailable, []));

        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.Equal(EstadoConsulta.SinRespuesta, resultado.Estado);
        Assert.Contains("HTTP 503", resultado.Detalle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConsultarAsync_CuerpoQueNoEsXml_DevuelveRespuestaNoReconocida()
    {
        var consulta = Crear(ManejadorHttpFalso.Responde(HttpStatusCode.OK, "<html><body>Mantenimiento"u8.ToArray()));

        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.Equal(EstadoConsulta.RespuestaNoReconocida, resultado.Estado);
        Assert.NotNull(resultado.Detalle);
    }

    [Fact]
    public async Task ConsultarAsync_ErrorDeRed_DevuelveSinRespuesta()
    {
        var consulta = Crear(new ManejadorHttpFalso(_ => throw new HttpRequestException("No se puede resolver el host.")));

        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.Equal(EstadoConsulta.SinRespuesta, resultado.Estado);
        Assert.Contains("No se puede resolver el host.", resultado.Detalle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConsultarAsync_SeVenceElTimeout_DevuelveSinRespuesta()
    {
        var consulta = Crear(ManejadorHttpFalso.NoResponde(), new EstatusCfdiSatOptions { Timeout = TimeSpan.FromMilliseconds(100) });

        var resultado = await consulta.ConsultarAsync(DatosPrueba.Datos());

        Assert.Equal(EstadoConsulta.SinRespuesta, resultado.Estado);
        Assert.Contains("Tiempo de espera agotado (0.1 s)", resultado.Detalle, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConsultarAsync_ElLlamadorCancela_LanzaOperationCanceledException()
    {
        var consulta = Crear(ManejadorHttpFalso.NoResponde());
        using var cancelacion = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consulta.ConsultarAsync(DatosPrueba.Datos(), cancelacion.Token));
    }

    [Fact]
    public async Task ConsultarAsync_DatosNulos_LanzaArgumentNullException()
    {
        var consulta = Crear(ManejadorHttpFalso.RespondeSat("vigente.xml"));

        await Assert.ThrowsAsync<ArgumentNullException>(() => consulta.ConsultarAsync(null!));
    }

    [Fact]
    public async Task ConsultarAsync_NoRegistraRfcNiTotal()
    {
        var logger = new LoggerEnMemoria<ConsultaEstatusCfdiSat>();
        var consulta = Crear(ManejadorHttpFalso.Responde(HttpStatusCode.InternalServerError, []), logger: logger);

        await consulta.ConsultarAsync(DatosPrueba.Datos());

        var (nivel, mensaje) = Assert.Single(logger.Entradas);
        Assert.Equal(LogLevel.Warning, nivel);
        Assert.Contains("766049bd-0000-4000-8000-000000000001", mensaje, StringComparison.Ordinal);
        Assert.DoesNotContain("EKU9003173C9", mensaje, StringComparison.Ordinal);
        Assert.DoesNotContain("URE180429TM6", mensaje, StringComparison.Ordinal);
        Assert.DoesNotContain("1595", mensaje, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_TimeoutInvalido_LanzaArgumentException(int milisegundos)
    {
        var opciones = new EstatusCfdiSatOptions { Timeout = TimeSpan.FromMilliseconds(milisegundos) };

        Assert.Throws<ArgumentException>(() => Crear(ManejadorHttpFalso.RespondeSat("vigente.xml"), opciones));
    }

    [Fact]
    public void Constructor_EndpointHttp_LanzaArgumentException()
    {
        var opciones = new EstatusCfdiSatOptions { Endpoint = new Uri("http://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc") };

        Assert.Throws<ArgumentException>(() => Crear(ManejadorHttpFalso.RespondeSat("vigente.xml"), opciones));
    }

    private ConsultaEstatusCfdiSat Crear(
        HttpMessageHandler manejador, EstatusCfdiSatOptions? opciones = null, ILogger<ConsultaEstatusCfdiSat>? logger = null)
    {
        var cliente = new HttpClient(manejador);
        _clientes.Add(cliente);
        return new ConsultaEstatusCfdiSat(
            cliente, Options.Create(opciones ?? new EstatusCfdiSatOptions()), logger ?? NullLogger<ConsultaEstatusCfdiSat>.Instance);
    }
}
