using System; // For ArgumentException (if we add parameter validation)

namespace EstatusCFDI.Sat.Helpers
{
    /// <summary>
    /// Proporciona métodos auxiliares para construir cadenas de consulta para el servicio del SAT.
    /// </summary>
    public static class SatQueryBuilder
    {
        /// <summary>
        /// Construye la cadena de parámetros requerida por el servicio de consulta de CFDI del SAT.
        /// El formato del monto total (tt) se ajusta a "000000,000000" (coma como separador decimal, 6 dígitos decimales, padding de ceros).
        /// </summary>
        /// <param name="rfcEmisor">El RFC (Registro Federal de Contribuyentes) del emisor del CFDI. No puede ser nulo o vacío.</param>
        /// <param name="rfcReceptor">El RFC (Registro Federal de Contribuyentes) del receptor del CFDI. No puede ser nulo o vacío.</param>
        /// <param name="montoTotal">El monto total del CFDI. Se formatea a 6 decimales con coma como separador.</param>
        /// <param name="uuid">El UUID (Universally Unique Identifier) o folio fiscal del CFDI. No puede ser nulo o vacío.</param>
        /// <returns>Una cadena formateada lista para ser usada en la consulta al servicio del SAT (ej: "?re=X&amp;rr=Y&amp;tt=Z&amp;id=U").</returns>
        /// <exception cref="ArgumentException">Lanzada si alguno de los parámetros RFC o UUID es nulo o una cadena vacía.</exception>
        public static string ConstruirCadenaParams(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid)
        {
            if (string.IsNullOrWhiteSpace(rfcEmisor))
                throw new ArgumentException("El RFC emisor no puede ser nulo o vacío.", nameof(rfcEmisor));
            if (string.IsNullOrWhiteSpace(rfcReceptor))
                throw new ArgumentException("El RFC receptor no puede ser nulo o vacío.", nameof(rfcReceptor));
            if (string.IsNullOrWhiteSpace(uuid))
                throw new ArgumentException("El UUID no puede ser nulo o vacío.", nameof(uuid));

            // Formatear el monto a "000000.000000" (6 decimales, padding de ceros) usando InvariantCulture para asegurar el punto como separador.
            string totalFormateado = montoTotal.ToString("000000.000000", System.Globalization.CultureInfo.InvariantCulture);
            // Reemplazar el punto (.) por coma (,) como lo requiere el SAT (según la evidencia del código comentado).
            totalFormateado = totalFormateado.Replace('.', ',');

            return $"?re={rfcEmisor}&rr={rfcReceptor}&tt={totalFormateado}&id={uuid}";
        }
    }
}
