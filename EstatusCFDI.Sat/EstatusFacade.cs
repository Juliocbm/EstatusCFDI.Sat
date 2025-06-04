using ConsultaCFDI; // For Acuse and ConsultaCFDIServiceClient
using EstatusCFDI.Sat.Interfaces;
using EstatusCFDI.Sat.Services;
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
            // El cliente WCF ConsultaCFDIServiceClient es IDisposable, por lo que se debe manejar su ciclo de vida.
            // ConsultaEstatusService actualmente toma ConsultaCFDIServiceClient en su constructor pero no maneja su disposición.
            // Para mantener la simpleza del Facade y asegurar la disposición del cliente WCF,
            // lo instanciamos aquí y lo pasamos. Una mejora futura podría ser que ConsultaEstatusService maneje IDisposable
            // si se pretende que ConsultaEstatusService sea de larga duración o inyectado.
            // Por ahora, para el Facade, este enfoque es directo.

            using var wcfClient = new ConsultaCFDIServiceClient();
            IConsultaEstatusService servicioConsulta = new ConsultaEstatusService(wcfClient);

            // La lógica de construir la cadena ya está encapsulada en servicioConsulta.ConsultarEstatusCfdiAsync
            return await servicioConsulta.ConsultarEstatusCfdiAsync(rfcEmisor, rfcReceptor, montoTotal, uuid);
        }
    }
}
