using EstatusCFDI.Sat;
using EstatusCFDI.Sat.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registro de EstatusCFDI.Sat en un contenedor de DI.
/// </summary>
public static class EstatusCfdiSatServiceCollectionExtensions
{
    /// <summary>Tope del cuerpo de respuesta: la del SAT pesa menos de 1 KB.</summary>
    internal const long TamanoMaximoRespuesta = 1024 * 1024;

    /// <summary>
    /// Registra <see cref="IConsultaEstatusCfdi"/> como cliente HTTP tipado.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configurar">
    /// Configuración opcional. También se respeta la registrada por otra vía, por ejemplo
    /// <c>services.Configure&lt;EstatusCfdiSatOptions&gt;(configuration.GetSection("EstatusCfdiSat"))</c>.
    /// </param>
    /// <returns>
    /// El <see cref="IHttpClientBuilder"/> del cliente, para agregar políticas propias (por ejemplo, un circuit breaker).
    /// La librería no reintenta.
    /// </returns>
    public static IHttpClientBuilder AddEstatusCfdiSat(this IServiceCollection services, Action<EstatusCfdiSatOptions>? configurar = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var opciones = services.AddOptions<EstatusCfdiSatOptions>();
        if (configurar is not null)
        {
            opciones.Configure(configurar);
        }

        opciones.ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<EstatusCfdiSatOptions>, ValidacionEstatusCfdiSatOptions>());

        return services.AddHttpClient<IConsultaEstatusCfdi, ConsultaEstatusCfdiSat>(cliente =>
        {
            // El límite por consulta lo aplica ConsultaEstatusCfdiSat con EstatusCfdiSatOptions.Timeout.
            cliente.Timeout = Timeout.InfiniteTimeSpan;
            cliente.MaxResponseContentBufferSize = TamanoMaximoRespuesta;
        });
    }
}
