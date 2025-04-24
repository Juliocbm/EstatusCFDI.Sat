using ConsultaCFDI;
using EstatusCFDI.Sat.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstatusCFDI.Sat
{
    /// <summary>
    /// Proporciona un acceso simplificado al servicio de consulta de estatus CFDI del SAT.
    /// </summary>
    public static class EstatusFacade
    {
        /// <summary>
        /// Consulta el estatus de un CFDI usando RFC del emisor, receptor, monto total y UUID.
        /// </summary>
        /// <param name="rfcEmisor">RFC del emisor</param>
        /// <param name="rfcReceptor">RFC del receptor</param>
        /// <param name="montoTotal">Monto total del CFDI</param>
        /// <param name="uuid">UUID del CFDI</param>
        /// <returns>Acuse del SAT con información del CFDI</returns>
        public static async Task<Acuse> ConsultarEstatusAsync(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid)
        {
            using var client = new ConsultaCFDIServiceClient();
            string cadena = SatQueryBuilder.ConstruirCadenaParams(rfcEmisor, rfcReceptor, montoTotal, uuid);
            return await client.ConsultaAsync(cadena);
        }
    }
}
