using ConsultaCFDI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstatusCFDI.Sat.Interfaces
{
    public interface IConsultaEstatusService
    {
        Task<Acuse> ConsultarEstatusCfdiAsync(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid);
    }
}
