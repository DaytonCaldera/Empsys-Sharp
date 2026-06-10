using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empsys.Core.Models
{
    public class Contrato
    {
        public int NumeroContrato { get; set; }
        public int ClienteId { get; set; }
        public decimal MontoPrestamo { get; set; }
        public decimal TasaInteresMensual { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public EstadoContrato Estado { get; set; } = EstadoContrato.ACTIVO;
    }
}
