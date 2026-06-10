using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Models;
using Empsys.Core.Services;
using Empsys.Core.Data;
using System;

namespace Empsys.App.ViewModels
{
    // ObservableObject avisa mágicamente a la vista cuando algo cambia
    public partial class FamiliaViewModel : ObservableObject
    {
        private readonly InventarioService _inventarioService;

        [ObservableProperty]
        private string _codigo = string.Empty;

        [ObservableProperty]
        private string _descripcion = string.Empty;

        [ObservableProperty]
        private string _mensaje = string.Empty; // Usaremos esto en lugar del intrusivo "ShowMessage" de Pascal

        public FamiliaViewModel()
        {
            var db = new EmpsysDbContext();

            // 🔥 TRUCO SENIOR: Esta línea soluciona el error de tu consola. 
            // Si el archivo SQLite no existe, lo crea al vuelo con todas sus tablas.
            db.Database.EnsureCreated();

            _inventarioService = new InventarioService(db);
        }

        // Este es el equivalente moderno y limpio a tu antiguo procedure btnGuardarClick
        [RelayCommand]
        public void GuardarFamilia()
        {
            try
            {
                var nuevaFamilia = new Familia
                {
                    Codigo = this.Codigo,
                    Descripcion = this.Descripcion
                };

                // Llamamos a la lógica de negocio que encapsulamos antes
                _inventarioService.GuardarFamilia(nuevaFamilia);

                Mensaje = $"¡La familia '{Descripcion}' se guardó en la base de datos SQLite!";

                // Limpiamos los campos
                Codigo = string.Empty;
                Descripcion = string.Empty;
            }
            catch (Exception ex)
            {
                // Si la regla de negocio falla (ej. descripción vacía), lo mostramos aquí
                Mensaje = $"Error: {ex.Message}";
            }
        }
    }
}