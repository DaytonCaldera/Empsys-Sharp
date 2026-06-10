using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Empsys.App.ViewModels
{
    // ObservableObject notifica automáticamente a la pantalla cuando cambian los datos
    public partial class ParamsViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _nombreEmpresa = "Empeños Central";

        [ObservableProperty]
        private double _tasaInteresBase = 5.5;

        // Esto reemplaza al evento "OnClick" del botón de Pascal/WinForms
        [RelayCommand]
        public void GuardarParametros()
        {
            // Aquí irá la llamada a la lógica de negocio corregida
            System.Diagnostics.Debug.WriteLine($"Guardando: {NombreEmpresa} con tasa {TasaInteresBase}%");
        }
    }
}