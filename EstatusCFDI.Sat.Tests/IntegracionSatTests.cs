using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EstatusCFDI.Sat.Tests;

/// <summary>
/// Pruebas contra el servicio real del SAT. Solo corren con <c>ESTATUSCFDI_SAT_INTEGRACION=1</c>; los CFDI reales
/// vienen de archivos locales FUERA del repositorio (no se versionan datos fiscales):
/// <list type="bullet">
/// <item><c>ESTATUSCFDI_SAT_CASOS</c>: archivo de texto con líneas <c>estado|rfcEmisor|rfcReceptor|total|uuid|fragmento</c>.</item>
/// <item><c>ESTATUSCFDI_SAT_XMLS</c>: carpeta con CFDI timbrados nombrados <c>&lt;estado&gt;__&lt;nombre&gt;.xml</c>.</item>
/// </list>
/// <c>estado</c> es el nombre de un valor de <see cref="EstadoConsulta"/>, por ejemplo <c>Cancelado</c>.
/// </summary>
[Trait("Categoria", "Integracion")]
public sealed class IntegracionSatTests : IDisposable
{
    private readonly HttpClient _cliente = new() { Timeout = Timeout.InfiniteTimeSpan };
    private readonly ConsultaEstatusCfdiSat _consulta;

    public IntegracionSatTests()
    {
        _consulta = new ConsultaEstatusCfdiSat(_cliente, Options.Create(new EstatusCfdiSatOptions()), NullLogger<ConsultaEstatusCfdiSat>.Instance);
    }

    public void Dispose() => _cliente.Dispose();

    [FactIntegracion]
    public async Task UuidInexistente_DevuelveNoEncontrado()
    {
        var datos = new DatosConsultaCfdi("EKU9003173C9", "XAXX010101000", 1.00m, Guid.Parse("00000000-1111-4222-8333-444444444444"));

        var resultado = await _consulta.ConsultarAsync(datos);

        Assert.Equal(EstadoConsulta.NoEncontrado, resultado.Estado);
    }

    [FactIntegracion(Variable = "ESTATUSCFDI_SAT_CASOS")]
    public async Task CasosDeArchivo_DevuelvenElEstadoEsperado()
    {
        var fallas = new List<string>();
        foreach (var linea in File.ReadAllLines(Environment.GetEnvironmentVariable("ESTATUSCFDI_SAT_CASOS")!))
        {
            if (string.IsNullOrWhiteSpace(linea) || linea.StartsWith('#'))
            {
                continue;
            }

            var c = linea.Split('|');
            var datos = new DatosConsultaCfdi(
                c[1], c[2], decimal.Parse(c[3], CultureInfo.InvariantCulture), Guid.Parse(c[4]), c.Length > 5 && c[5].Length > 0 ? c[5] : null);
            var resultado = await _consulta.ConsultarAsync(datos);
            if (resultado.Estado != Enum.Parse<EstadoConsulta>(c[0]))
            {
                fallas.Add($"{datos.Uuid}: se esperaba {c[0]} y llegó {resultado.Estado} ({resultado.Acuse?.CodigoEstatus ?? resultado.Detalle})");
            }
        }

        Assert.Empty(fallas);
    }

    [FactIntegracion(Variable = "ESTATUSCFDI_SAT_XMLS")]
    public async Task XmlsDeCarpeta_DevuelvenElEstadoEsperado()
    {
        var fallas = new List<string>();
        foreach (var archivo in Directory.GetFiles(Environment.GetEnvironmentVariable("ESTATUSCFDI_SAT_XMLS")!, "*.xml"))
        {
            var esperado = Enum.Parse<EstadoConsulta>(Path.GetFileName(archivo).Split("__")[0]);
            var resultado = await _consulta.ConsultarAsync(DatosConsultaCfdi.DesdeXml(File.ReadAllBytes(archivo)));
            if (resultado.Estado != esperado)
            {
                fallas.Add($"{Path.GetFileName(archivo)}: llegó {resultado.Estado} ({resultado.Acuse?.CodigoEstatus ?? resultado.Detalle})");
            }
        }

        Assert.Empty(fallas);
    }
}

/// <summary>
/// Prueba que solo corre con <c>ESTATUSCFDI_SAT_INTEGRACION=1</c> y, si se indica, con <see cref="Variable"/> definida.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactIntegracionAttribute : FactAttribute
{
    private string? _variable;

    public FactIntegracionAttribute() => Skip = MotivoOmision(null);

    /// <summary>Variable de entorno adicional que debe existir.</summary>
    public string? Variable
    {
        get => _variable;
        set
        {
            _variable = value;
            Skip = MotivoOmision(value);
        }
    }

    private static string? MotivoOmision(string? variable)
    {
        if (Environment.GetEnvironmentVariable("ESTATUSCFDI_SAT_INTEGRACION") != "1")
        {
            return "Prueba contra el SAT real: define ESTATUSCFDI_SAT_INTEGRACION=1.";
        }

        return variable is not null && string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable))
            ? $"Define {variable}."
            : null;
    }
}
