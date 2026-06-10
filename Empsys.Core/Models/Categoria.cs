namespace Empsys.Core.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public int FamiliaId { get; set; }
        public string Descripcion { get; set; } = string.Empty;

        // Propiedad de navegación
        public Familia Familia { get; set; } = null!;
    }
}