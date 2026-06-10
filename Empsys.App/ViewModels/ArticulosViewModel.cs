using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Data;
using Empsys.Core.Models;
using Empsys.Core.Services;

namespace Empsys.App.ViewModels
{
    public partial class ArticuloViewModel : ObservableObject
    {
        private readonly InventarioService _inventarioService;

        // Colecciones que la vista (XAML) estará vigilando
        public ObservableCollection<Familia> Familias { get; } = new();
        public ObservableCollection<Categoria> Categorias { get; } = new();

        // Propiedad calculada que lee el ComboBox de categorías para habilitarse o deshabilitarse
        public bool HasCategories => Categorias.Count > 0;

        // Propiedades del formulario enlazadas a los TextBox
        [ObservableProperty]
        private string _descripcion = string.Empty;

        [ObservableProperty]
        private string _precioEstimadoTexto = string.Empty;

        [ObservableProperty]
        private string _mensaje = string.Empty;

        // IDs usando tipos Nullable (?) para evitar excepciones al limpiar la selección (poner en null)
        [ObservableProperty]
        private int? _selectedFamiliaId;

        [ObservableProperty]
        private int? _selectedCategoriaId;

        public ArticuloViewModel()
        {
            var db = new EmpsysDbContext();
            _inventarioService = new InventarioService(db);

            // Carga inicial de familias al abrir la pantalla
            CargarFamilias();
        }

        /// <summary>
        /// Obtiene todas las familias registradas en SQLite.
        /// </summary>
        private void CargarFamilias()
        {
            try
            {
                Familias.Clear();
                var listaFamilias = _inventarioService.ObtenerFamilias();
                foreach (var familia in listaFamilias)
                {
                    Familias.Add(familia);
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar familias: {ex.Message}";
            }
        }

        /// <summary>
        /// Método reactivo que se ejecuta automáticamente cuando cambia la Familia seleccionada.
        /// </summary>
        partial void OnSelectedFamiliaIdChanged(int? value)
        {
            // Limpiamos de forma segura las categorías previas
            Categorias.Clear();
            SelectedCategoriaId = null;

            // Al vaciar la lista de categorías, forzamos a actualizar la propiedad calculada
            OnPropertyChanged(nameof(HasCategories));

            if (value == null || value <= 0) return;

            try
            {
                // Buscamos las categorías dependientes en la base de datos
                var listaCategorias = _inventarioService.ObtenerCategoriasPorFamilia(value.Value);
                foreach (var categoria in listaCategorias)
                {
                    Categorias.Add(categoria);
                }

                // Notificamos explícitamente a la vista que 'HasCategories' cambió su valor
                OnPropertyChanged(nameof(HasCategories));

                if (Categorias.Count == 0)
                {
                    Mensaje = "Esta familia aún no tiene categorías registradas.";
                }
                else
                {
                    Mensaje = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cargar categorías: {ex.Message}";
            }
        }

        /// <summary>
        /// Procesa el guardado del Artículo con validaciones seguras de tipo y negocio.
        /// </summary>
        [RelayCommand]
        public void GuardarArticulo()
        {
            try
            {
                // 1. Validaciones de Interfaz
                if (string.IsNullOrWhiteSpace(Descripcion))
                {
                    Mensaje = "Error: La descripción del artículo es obligatoria.";
                    return;
                }

                if (SelectedFamiliaId == null || SelectedCategoriaId == null || SelectedCategoriaId <= 0)
                {
                    Mensaje = "Error: Debe seleccionar una Familia y una Categoría válidas.";
                    return;
                }

                if (!decimal.TryParse(PrecioEstimadoTexto, out decimal precio) || precio < 0)
                {
                    Mensaje = "Error: El precio debe ser un número válido mayor o igual a cero.";
                    return;
                }

                // 2. Mapeo a la entidad pura de dominio
                var nuevoArticulo = new Articulo
                {
                    Descripcion = this.Descripcion,
                    FamiliaId = this.SelectedFamiliaId.Value,
                    CategoriaId = this.SelectedCategoriaId.Value,
                    Estado = EstadoArticulo.INVENTARIO
                };

                // 3. Persistencia mediante el Core
                _inventarioService.GuardarArticulo(nuevoArticulo);

                // 4. Éxito y restauración limpia de controles
                Mensaje = $"¡Artículo '{Descripcion}' registrado con éxito en el inventario!";
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                Mensaje = $"Error de negocio: {ex.Message}";
            }
        }

        /// <summary>
        /// Restablece los campos de forma segura sin causar errores de PropertyChanged en WinUI 3.
        /// </summary>
        private void LimpiarFormulario()
        {
            Descripcion = string.Empty;
            PrecioEstimadoTexto = string.Empty;

            // Asignar null restablece visualmente los ComboBox de manera segura
            SelectedFamiliaId = null;
            SelectedCategoriaId = null;
            Categorias.Clear();

            // Forzamos la actualización final para deshabilitar el combo de categorías
            OnPropertyChanged(nameof(HasCategories));
        }
    }
}