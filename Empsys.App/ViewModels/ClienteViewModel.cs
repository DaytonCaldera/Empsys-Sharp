using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Data;
using Empsys.Core.Models;
using Empsys.Core.Services;

namespace Empsys.App.ViewModels
{
    public partial class ClienteViewModel : ObservableObject
    {
        private readonly ClienteService _clienteService;

        // Propiedades de la pantalla
        [ObservableProperty]
        private string _cedula = string.Empty;

        [ObservableProperty]
        private string _nombreCompleto = string.Empty;

        // Lista de opciones para el ComboBox basadas en el Enum que creamos al principio
        public ObservableCollection<NivelRiesgoCliente> NivelesRiesgo { get; } = new()
        {
            NivelRiesgoCliente.Confiable,
            NivelRiesgoCliente.Problematico
        };

        [ObservableProperty]
        private NivelRiesgoCliente _selectedNivelRiesgo = NivelRiesgoCliente.Confiable;

        [ObservableProperty]
        private string _mensaje = string.Empty;

        // Propiedad visual para saber si estamos editando
        [ObservableProperty]
        private bool _esEdicion;

        public ClienteViewModel()
        {
            var db = new EmpsysDbContext();
            _clienteService = new ClienteService(db);
        }

        [RelayCommand]
        public void BuscarCliente()
        {
            Mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(Cedula))
            {
                Mensaje = "Ingrese una cédula para realizar la búsqueda.";
                return;
            }

            var cliente = _clienteService.BuscarPorCedula(Cedula);

            if (cliente != null)
            {
                // Lo encontramos: Llenamos los datos para editar
                NombreCompleto = cliente.NombreCompleto;
                SelectedNivelRiesgo = cliente.NivelRiesgo;
                EsEdicion = true;
                Mensaje = "Cliente encontrado. Puede actualizar sus datos.";
            }
            else
            {
                // No existe: Preparamos para un alta nueva
                NombreCompleto = string.Empty;
                SelectedNivelRiesgo = NivelRiesgoCliente.Confiable;
                EsEdicion = false;
                Mensaje = "Cliente nuevo. Llene el nombre para registrarlo.";
            }
        }

        [RelayCommand]
        public void GuardarCliente()
        {
            try
            {
                var cliente = new Cliente
                {
                    Cedula = this.Cedula,
                    NombreCompleto = this.NombreCompleto,
                    NivelRiesgo = this.SelectedNivelRiesgo
                };

                _clienteService.GuardarOActualizar(cliente);

                Mensaje = EsEdicion ? "¡Datos del cliente actualizados!" : "¡Nuevo cliente registrado con éxito!";
                EsEdicion = true; // Después de guardar, el cliente ya existe
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al guardar: {ex.Message}";
            }
        }

        [RelayCommand]
        public void LimpiarPantalla()
        {
            Cedula = string.Empty;
            NombreCompleto = string.Empty;
            SelectedNivelRiesgo = NivelRiesgoCliente.Confiable;
            Mensaje = string.Empty;
            EsEdicion = false;
        }
    }
}