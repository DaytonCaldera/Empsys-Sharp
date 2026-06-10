using System.Collections.Generic;

namespace Empsys.Core.Models
{
    public class Familia
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Relación: Una familia (ej. Joyería) tiene muchas categorías (ej. Anillos, Cadenas)
        public List<Categoria> Categorias { get; set; } = new();
    }
}