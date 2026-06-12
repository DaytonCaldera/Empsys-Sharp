using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Data;
using Empsys.Core.Models;
using Empsys.Core.Services;

namespace Empsys.App.ViewModels
{
    public partial class ContratoViewModel : ObservableObject
    {
        private readonly ContratoService _contratoService;
        private readonly ClienteService _clienteService;
        private readonly InventarioService _inventarioService;

        [ObservableProperty] private string _cedulaBusqueda = string.Empty;
        [ObservableProperty] private Cliente _clienteActual;
        [ObservableProperty] private string _nombreCliente = "Ingrese cédula...";

        // Colecciones para las 3 cascadas
        public ObservableCollection<Familia> Familias { get; } = new();
        public ObservableCollection<Categoria> Categorias { get; } = new();
        public ObservableCollection<Articulo> ArticulosCatalogo { get; } = new();

        public bool HasCategorias => Categorias.Count > 0;
        public bool HasArticulos => ArticulosCatalogo.Count > 0;

        [ObservableProperty] private int? _selectedFamiliaId;
        [ObservableProperty] private int? _selectedCategoriaId;
        [ObservableProperty] private int? _selectedArticuloId;
        [ObservableProperty] private string _valorArticuloTexto = string.Empty;

        // La tabla temporal de la pantalla
        public ObservableCollection<Inventario> ArticulosEnGrid { get; } = new();
        [ObservableProperty] private Inventario _articuloSeleccionadoEnGrid;

        [ObservableProperty] private string _descripcionContrato = string.Empty;
        [ObservableProperty] private string _tasaInteresTexto = "10";
        public decimal TotalPrestamo => ArticulosEnGrid.Sum(a => a.PrecioEstimado);

        [ObservableProperty] private string _mensaje = string.Empty;

        public ContratoViewModel()
        {
            var db = new EmpsysDbContext();
            _contratoService = new ContratoService(db);
            _clienteService = new ClienteService(db);
            _inventarioService = new InventarioService(db);

            foreach (var f in _inventarioService.ObtenerFamilias()) Familias.Add(f);
            ArticulosEnGrid.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalPrestamo));
        }

        // Cascada 1: Al cambiar Familia -> Carga Categorías
        partial void OnSelectedFamiliaIdChanged(int? value)
        {
            Categorias.Clear();
            SelectedCategoriaId = null;
            OnPropertyChanged(nameof(HasCategorias));

            if (value > 0)
            {
                foreach (var c in _inventarioService.ObtenerCategoriasPorFamilia(value.Value)) Categorias.Add(c);
                OnPropertyChanged(nameof(HasCategorias));
            }
        }

        // Cascada 2: Al cambiar Categoría -> Carga Artículos (Punto 2)
        partial void OnSelectedCategoriaIdChanged(int? value)
        {
            ArticulosCatalogo.Clear();
            SelectedArticuloId = null;
            OnPropertyChanged(nameof(HasArticulos));

            if (value > 0)
            {
                foreach (var a in _inventarioService.ObtenerArticulosPorCategoria(value.Value)) ArticulosCatalogo.Add(a);
                OnPropertyChanged(nameof(HasArticulos));
            }
        }

        // Cascada 3: Al cambiar Artículo -> Autorellena el precio estimado en la UI
        partial void OnSelectedArticuloIdChanged(int? value)
        {
            if (value > 0)
            {
                var art = ArticulosCatalogo.FirstOrDefault(a => a.Id == value.Value);
            }
            else
            {
                ValorArticuloTexto = string.Empty;
            }
        }

        [RelayCommand]
        public void BuscarCliente()
        {
            ClienteActual = _clienteService.BuscarPorCedula(CedulaBusqueda);
            NombreCliente = ClienteActual != null ? ClienteActual.NombreCompleto : "Cliente no encontrado.";
        }

        [RelayCommand]
        public void AgregarArticuloAlGrid()
        {
            if (SelectedArticuloId == null || SelectedArticuloId <= 0) return;

            var artOriginal = ArticulosCatalogo.FirstOrDefault(a => a.Id == SelectedArticuloId.Value);
            if (artOriginal != null)
            {
                Inventario inventarioNuevo = new Inventario
                {
                    ArticuloId = artOriginal.Id,
                    Articulo = artOriginal,
                    PrecioEstimado = decimal.TryParse(ValorArticuloTexto, out decimal precio) ? precio : 0
                };
                // Agregamos una copia del artículo de catálogo a nuestra mesa de trabajo
                ArticulosEnGrid.Add(inventarioNuevo);

                // Reseteamos el combo de artículo para la siguiente inclusión
                SelectedArticuloId = null;
                ValorArticuloTexto = string.Empty;
            }
        }

        [RelayCommand]
        public void QuitarArticuloDelGrid()
        {
            if (ArticuloSeleccionadoEnGrid != null) ArticulosEnGrid.Remove(ArticuloSeleccionadoEnGrid);
        }

        [RelayCommand]
        public void EmitirContrato()
        {
            try
            {
                if (!decimal.TryParse(TasaInteresTexto, out decimal tasa)) return;

                var contrato = _contratoService.CrearNuevoContrato(ClienteActual, ArticulosEnGrid.ToList(), DescripcionContrato, tasa);

                Mensaje = $"¡CONTRATO #{contrato.NumeroContrato} EMITIDO CON ÉXITO!";
                ArticulosEnGrid.Clear();
                DescripcionContrato = string.Empty;
                CedulaBusqueda = string.Empty;
                NombreCliente = "Ingrese cédula...";
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message} \n {ex.StackTrace}";
            }
        }
    }
}