using System;
using System.Collections.Generic;

namespace Empsys.Core.Models
{
    public class Contrato
    {
        public int NumeroContrato { get; set; }
        public int ClienteId { get; set; }

        // Relación: Un contrato tiene una lista de artículos
        public List<Inventario> Inventarios { get; set; } = new();

        public string Descripcion { get; set; } = string.Empty;
        public decimal MontoPrestamo { get; set; }
        public decimal TasaInteresMensual { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }

        public EstadoContrato Estado { get; set; } = EstadoContrato.ACTIVO;
    }
}