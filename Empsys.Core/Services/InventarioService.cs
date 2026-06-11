using System;
using System.Collections.Generic;
using System.Linq;
using Empsys.Core.Data;
using Empsys.Core.Models;

namespace Empsys.Core.Services
{
    public class InventarioService
    {
        private readonly EmpsysDbContext _dbContext;

        // Inyectamos la base de datos (EF Core) al servicio
        public InventarioService(EmpsysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Método para obtener las listas para los Dropdowns (Combos)
        public List<Familia> ObtenerFamilias() => _dbContext.Familias.ToList();

        public List<Categoria> ObtenerCategoriasPorFamilia(int familiaId)
            => _dbContext.Categorias.Where(c => c.FamiliaId == familiaId).ToList();

        public void GuardarFamilia(Familia nuevaFamilia)
        {
            // 1. Validaciones de negocio (Separadas de la UI)
            if (string.IsNullOrWhiteSpace(nuevaFamilia.Descripcion))
                throw new ArgumentException("La descripción de la familia no puede estar vacía.");

            // 2. Persistencia limpia mediante EF Core
            _dbContext.Familias.Add(nuevaFamilia);

            // 3. Guarda los cambios en SQLite (Reemplaza al antiguo .Post de Pascal)
            _dbContext.SaveChanges();
        }

        public void GuardarCategoria(Categoria nuevaCategoria)
        {
            if (string.IsNullOrWhiteSpace(nuevaCategoria.Descripcion))
                throw new ArgumentException("La descripción de la categoría no puede estar vacía.");

            if (nuevaCategoria.FamiliaId <= 0)
                throw new ArgumentException("Debe seleccionar una Familia válida para esta categoría.");

            _dbContext.Categorias.Add(nuevaCategoria);
            _dbContext.SaveChanges();
        }

        // --- NUEVO MÉTODO AGREGADO ---
        public void GuardarArticulo(Articulo nuevoArticulo)
        {
            // Validaciones de negocio puras
            if (string.IsNullOrWhiteSpace(nuevoArticulo.Descripcion))
                throw new ArgumentException("La descripción del artículo no puede estar vacía.");

            if (nuevoArticulo.FamiliaId <= 0 || nuevoArticulo.CategoriaId <= 0)
                throw new ArgumentException("El artículo debe estar asociado a una Familia y una Categoría válidas.");

            // Persistencia en SQLite
            _dbContext.Articulos.Add(nuevoArticulo);
            _dbContext.SaveChanges();
        }

        public List<Articulo> ObtenerArticulosPorCategoria(int categoriaId) => _dbContext.Articulos.Where(a => a.CategoriaId == categoriaId).ToList();
    }
}