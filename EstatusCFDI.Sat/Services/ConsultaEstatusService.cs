using ConsultaCFDI;
using EstatusCFDI.Sat.Helpers;
using EstatusCFDI.Sat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel; // Required for FaultException, TimeoutException
using System.Text;
using System.Threading.Tasks;

namespace EstatusCFDI.Sat.Services
{
    /// <summary>
    /// Implementación del servicio para consultar el estatus de CFDI utilizando el cliente WCF del SAT.
    /// </summary>
    public class ConsultaEstatusService : IConsultaEstatusService
    {
        private readonly ConsultaCFDIServiceClient _client;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ConsultaEstatusService"/>.
        /// </summary>
        /// <param name="client">El cliente WCF (<see cref="ConsultaCFDIServiceClient"/>) para comunicarse con el servicio del SAT.</param>
        public ConsultaEstatusService(ConsultaCFDIServiceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc cref="IConsultaEstatusService.ConsultarEstatusCfdiAsync"/>
        public async Task<Acuse> ConsultarEstatusCfdiAsync(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid)
        {
            // Validaciones de parámetros de entrada (ejemplo)
            if (string.IsNullOrWhiteSpace(rfcEmisor))
                throw new ArgumentException("El RFC emisor no puede ser nulo o vacío.", nameof(rfcEmisor));
            if (string.IsNullOrWhiteSpace(rfcReceptor))
                throw new ArgumentException("El RFC receptor no puede ser nulo o vacío.", nameof(rfcReceptor));
            if (string.IsNullOrWhiteSpace(uuid))
                throw new ArgumentException("El UUID no puede ser nulo o vacío.", nameof(uuid));
            // Podrían añadirse más validaciones (longitud de RFCs, formato de UUID, monto no negativo, etc.)

            try
            {
                string cadena = SatQueryBuilder.ConstruirCadenaParams(rfcEmisor, rfcReceptor, montoTotal, uuid);
                return await _client.ConsultaAsync(cadena);
            }
            catch (FaultException ex)
            {
                // TODO: Considerar logging o excepción personalizada que incluya más detalles.
                // Por ahora, simplemente relanzamos para mantener el contrato de la interfaz.
                throw;
            }
            catch (TimeoutException ex)
            {
                // TODO: Considerar logging o excepción personalizada.
                throw;
            }
            // Opcionalmente, capturar CommunicationException si se quiere un manejo específico,
            // o dejar que se propague (ya está declarada en la interfaz).
            // catch (CommunicationException ex)
            // {
            //     // TODO: Logging o excepción personalizada.
            //     throw;
            // }
        }
    }
}
