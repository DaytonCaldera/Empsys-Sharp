namespace Empsys.Core.Models
{
    public class Articulo
    {
        public int Id { get; set; }
        public int NumeroContrato { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        public int FamiliaId { get; set; }
        public Familia Familia { get; set; } = null!; // Propiedad de navegación

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!; // Propiedad de navegación

        public EstadoArticulo Estado { get; set; } = EstadoArticulo.INVENTARIO;
    }
}