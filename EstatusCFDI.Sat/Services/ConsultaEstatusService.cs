using ConsultaCFDI;
using EstatusCFDI.Sat.Helpers;
using EstatusCFDI.Sat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstatusCFDI.Sat.Services
{
    public class ConsultaEstatusService : IConsultaEstatusService
    {
        private readonly ConsultaCFDIServiceClient _client;

        public ConsultaEstatusService(ConsultaCFDIServiceClient client)
        {
            _client = client;
        }

        public async Task<Acuse> ConsultarEstatusCfdiAsync(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid)
        {
            string cadena = SatQueryBuilder.ConstruirCadenaParams(rfcEmisor, rfcReceptor, montoTotal, uuid);
            return await _client.ConsultaAsync(cadena);
        }
    }
}
