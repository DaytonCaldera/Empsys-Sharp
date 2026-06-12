using System;

namespace Empsys.Core.Models
{
    public enum TipoPago { Intereses, Cancelacion, AbonoCapital }

    public class Pago
    {
        public int Id { get; set; }
        
        public int NumeroContrato { get; set; }
        public Contrato Contrato { get; set; } = null!;

        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.Now;
        
        // --- NUEVA PROPIEDAD PARA EL HISTORIAL EXACTO ---
        public DateTime? NuevaFechaVencimiento { get; set; } 
        
        // ... el resto de propiedades siguen igual ...
        public int MesesPagados { get; set; } // Agregamos esto también para auditoría
        public TipoPago Tipo { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}