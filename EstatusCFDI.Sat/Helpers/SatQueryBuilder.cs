using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstatusCFDI.Sat.Helpers
{
    public static class SatQueryBuilder
    {
        public static string ConstruirCadenaParams(string rfcEmisor, string rfcReceptor, decimal montoTotal, string uuid)
        {
            string totalFormateado = montoTotal.ToString("000000.000000").Replace('.', ',');
            return $"?re={rfcEmisor}&rr={rfcReceptor}&tt={totalFormateado}&id={uuid}";
        }
    }
}
