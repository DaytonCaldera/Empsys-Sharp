using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Data;
using Empsys.Core.Services;

namespace Empsys.App.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        // Evento que dispararemos cuando la validación sea correcta
        public Action OnLoginExitoso { get; set; }

        public LoginViewModel()
        {
            var db = new EmpsysDbContext();
            _authService = new AuthService(db);
        }

        [RelayCommand]
        public void Ingresar()
        {
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MensajeError = "Por favor, ingrese su usuario y contraseña.";
                return;
            }

            var usuarioValido = _authService.Autenticar(Username, Password);

            if (usuarioValido != null)
            {
                // ¡Éxito! Avisamos a la UI que puede desbloquear el sistema
                OnLoginExitoso?.Invoke();
            }
            else
            {
                MensajeError = "Credenciales incorrectas. Intente nuevamente.";
            }
        }
    }
}