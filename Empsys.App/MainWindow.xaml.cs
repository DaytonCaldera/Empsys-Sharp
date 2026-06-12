using Empsys.App.ViewModels;
using Empsys.App.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Empsys.App
{
    public partial class MainWindow : Window
    {
        public LoginViewModel LoginViewModel { get; } = new LoginViewModel();

        public MainWindow()
        {
            this.InitializeComponent();

            // Suscribimos la acción de éxito al ViewModel
            LoginViewModel.OnLoginExitoso = DesbloquearSistema;
        }

        private void DesbloquearSistema()
        {
            // Ocultamos la capa de login
            CapaLogin.Visibility = Visibility.Collapsed;

            // Mostramos el menú del sistema
            NavPrincipal.Visibility = Visibility.Visible;

            // Cargamos la primera página por defecto al entrar
            ContenedorPrincipal.Navigate(typeof(ClienteView));
        }

        /// <summary>
        /// Evento que captura el clic en el menú lateral e intercambia las vistas en el Frame.
        /// </summary>
        private void NavPrincipal_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Aquí podrías cargar una vista de configuraciones (Parámetros) en el futuro
                sender.Header = "Configuración del Sistema";
                return;
            }

            // Leemos el Tag que le pusimos a cada item en el archivo XAML
            var itemSeleccionado = args.SelectedItemContainer as NavigationViewItem;
            if (itemSeleccionado == null) return;

            string tag = itemSeleccionado.Tag.ToString();

            // Enrutador directo: según el Tag, inyecta el tipo de página correspondiente
            switch (tag)
            {
                case "ContratosPage":
                    ContenedorPrincipal.Navigate(typeof(ContratoView));
                    sender.Header = "Gestión de Contratos";
                    break;
                case "RenovacionesPage":
                    ContenedorPrincipal.Navigate(typeof(CajaView));
                    sender.Header = "Gestión de Contratos";
                    break;
                case "ClientesPage":
                    ContenedorPrincipal.Navigate(typeof(ClienteView));
                    sender.Header = "Gestión de Clientes";
                    break;

                case "FamiliasPage":
                    ContenedorPrincipal.Navigate(typeof(FamiliaView));
                    sender.Header = "Familias de Artículos";
                    break;

                case "CategoriasPage":
                    ContenedorPrincipal.Navigate(typeof(CategoriaView));
                    sender.Header = "Categorías de Artículos";
                    break;

                case "ArticulosPage":
                    ContenedorPrincipal.Navigate(typeof(ArticuloView));
                    sender.Header = "Inventario de Artículos";
                    break;
            }
        }
    }
}