using Microsoft.UI.Xaml.Controls;
using Empsys.App.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Empsys.App.Views
{
    public sealed partial class CategoriaView : Page
    {
        public CategoriaViewModel ViewModel { get; set; }
        public CategoriaView()
        {
            InitializeComponent();
            ViewModel = new CategoriaViewModel();
        }
    }
}
