using System.Reflection;

namespace EstatusCFDI.Sat.Tests.Helpers;

/// <summary>
/// Lee los archivos de TestData (recursos incrustados). Los CFDI son sintéticos; las respuestas del SAT son reales
/// (capturadas el 2026-10-15) y no traen datos fiscales.
/// </summary>
internal static class DatosPrueba
{
    private static readonly Assembly Ensamblado = typeof(DatosPrueba).Assembly;

    public static byte[] Leer(string carpeta, string archivo)
    {
        var sufijo = $".TestData.{carpeta}.{archivo}";
        var nombre = Ensamblado.GetManifestResourceNames().SingleOrDefault(n => n.EndsWith(sufijo, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"No existe el recurso de prueba {carpeta}/{archivo}.");

        using var recurso = Ensamblado.GetManifestResourceStream(nombre)!;
        using var copia = new MemoryStream();
        recurso.CopyTo(copia);
        return copia.ToArray();
    }

    public static byte[] Cfdi(string archivo) => Leer("Cfdi", archivo);

    public static byte[] RespuestaSat(string archivo) => Leer("Sat", archivo);

    /// <summary>Datos válidos de ejemplo (RFC de pruebas del SAT).</summary>
    public static DatosConsultaCfdi Datos(string rfcReceptor = "URE180429TM6", string? fragmentoSello = "Q2QxMjM0") =>
        new("EKU9003173C9", rfcReceptor, 1595.00m, Guid.Parse("766049bd-0000-4000-8000-000000000001"), fragmentoSello);
}
