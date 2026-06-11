namespace Empsys.Core.Models
{
    public class Inventario
    {
        public int Id { get; set; }

        public int ArticuloId { get; set; }
        public Articulo Articulo { get; set; } = null!;

        public decimal PrecioEstimado { get; set; }

        public int NumeroContrato { get; set; }
        public Contrato Contrato { get; set; } = null!;

        public EstadoArticulo Estado { get; set; } = EstadoArticulo.EMPENIADO;
    }
}