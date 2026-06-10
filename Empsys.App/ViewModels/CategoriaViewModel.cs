using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Data;
using Empsys.Core.Models;
using Empsys.Core.Services;

namespace Empsys.App.ViewModels
{
    public partial class CategoriaViewModel : ObservableObject
    {
        private readonly InventarioService _inventarioService;

        public ObservableCollection<Familia> Familias { get; } = new();

        [ObservableProperty]
        private string _descripcion = string.Empty;

        [ObservableProperty]
        private int? _selectedFamiliaId;

        [ObservableProperty]
        private string _mensaje = string.Empty;

        public CategoriaViewModel()
        {
            var db = new EmpsysDbContext();
            _inventarioService = new InventarioService(db);
            CargarFamilias();
        }

        private void CargarFamilias()
        {
            try
            {
                Familias.Clear();
                var lista = _inventarioService.ObtenerFamilias();
                foreach (var fam in lista)
                {
                    Familias.Add(fam);
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar familias: {ex.Message}";
            }
        }

        [RelayCommand]
        public void GuardarCategoria()
        {
            try
            {
                if (!SelectedFamiliaId.HasValue || SelectedFamiliaId.Value <= 0)
                {
                    Mensaje = "Error: Debe seleccionar una familia.";
                    return;
                }

                var nuevaCategoria = new Categoria
                {
                    Descripcion = this.Descripcion,
                    FamiliaId = this.SelectedFamiliaId.Value
                };

                _inventarioService.GuardarCategoria(nuevaCategoria);

                Mensaje = $"¡Categoría '{Descripcion}' guardada con éxito!";
                Descripcion = string.Empty;
                SelectedFamiliaId = null; // or = 0 if you prefer
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
            }
        }
    }
}