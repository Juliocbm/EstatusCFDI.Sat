using Microsoft.Extensions.Options;

namespace EstatusCFDI.Sat.Internal;

/// <summary>
/// Reglas de <see cref="EstatusCfdiSatOptions"/>. Las usa el registro en DI y también el constructor del cliente,
/// para que el uso sin DI valide lo mismo.
/// </summary>
internal sealed class ValidacionEstatusCfdiSatOptions : IValidateOptions<EstatusCfdiSatOptions>
{
    private static readonly TimeSpan TimeoutMaximo = TimeSpan.FromMilliseconds(int.MaxValue);

    public ValidateOptionsResult Validate(string? name, EstatusCfdiSatOptions options)
    {
        var errores = Errores(options);
        return errores.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errores);
    }

    public static List<string> Errores(EstatusCfdiSatOptions opciones)
    {
        var errores = new List<string>();

        if (opciones.Endpoint is null || !opciones.Endpoint.IsAbsoluteUri || opciones.Endpoint.Scheme != Uri.UriSchemeHttps)
        {
            errores.Add($"{nameof(EstatusCfdiSatOptions)}.{nameof(EstatusCfdiSatOptions.Endpoint)} debe ser una URL https absoluta.");
        }

        if (opciones.Timeout <= TimeSpan.Zero || opciones.Timeout > TimeoutMaximo)
        {
            errores.Add($"{nameof(EstatusCfdiSatOptions)}.{nameof(EstatusCfdiSatOptions.Timeout)} debe ser mayor que cero y menor que {TimeoutMaximo}.");
        }

        return errores;
    }
}
