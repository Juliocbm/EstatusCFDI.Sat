using ConsultaCFDI; // Para Acuse
using System.ServiceModel; // Para FaultException, TimeoutException
using System.Threading.Tasks;

namespace EstatusCFDI.Sat.Interfaces
{
    /// <summary>
    /// Define el contrato para el servicio de consulta de estatus de CFDI.
    /// </summary>
    public interface IConsultaEstatusService
    {
        /// <summary>
        /// Consulta el estatus de un Comprobante Fiscal Digital por Internet (CFDI) en el servicio del SAT.
        /// </summary>
        /// <param name="rfcEmisor">El RFC (Registro Federal de Contribuyentes) del emisor del CFDI.</param>
        /// <param name="rfcReceptor">El RFC (Registro Federal de Contribuyentes) del receptor del CFDI.</param>
        /// <param name="montoTotal">El monto total del CFDI.</param>
        /// <param name="uuid">El UUID (Universally Unique Identifier) o folio fiscal del CFDI.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado de la tarea es un objeto <see cref="Acuse"/> que contiene la respuesta del SAT.</returns>
        /// <exception cref="FaultException">Lanzada si el servicio del SAT retorna un error SOAP (falla).</exception>
        /// <exception cref="TimeoutException">Lanzada si la llamada al servicio del SAT excede el tiempo de espera configurado.</exception>
        /// <exception cref="System.ServiceModel.CommunicationException">Lanzada si ocurre un error de comunicación general con el servicio del SAT.</exception>
        Task<Acuse> ConsultarEstatusCfdiAsync(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid);
    }
}
