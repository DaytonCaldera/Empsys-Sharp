using Microsoft.UI.Xaml.Controls;
using Empsys.App.ViewModels;

namespace Empsys.App.Views
{
    public sealed partial class FamiliaView : Page
    {
        // Esta propiedad le permite al archivo XAML conocer dónde está la lógica
        public FamiliaViewModel ViewModel { get; }

        public FamiliaView()
        {
            this.InitializeComponent();

            // Instanciamos el ViewModel que creamos en el Core/Servicios
            ViewModel = new FamiliaViewModel();
        }
    }
}