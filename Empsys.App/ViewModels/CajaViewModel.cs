using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Empsys.Core.Data;
using Empsys.Core.Models;
using Empsys.Core.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Empsys.App.ViewModels
{
    public partial class CajaViewModel : ObservableObject
    {
        private readonly CajaService _cajaService;
        private readonly FinanzasService _finanzasService;
        private readonly EmpsysDbContext _dbContext;

        // --- BÚSQUEDA ---
        [ObservableProperty] private string _numeroContratoBusqueda = string.Empty;
        [ObservableProperty] private string _mensaje = string.Empty;

        // --- DATOS DEL CONTRATO ---
        [ObservableProperty] private Contrato _contratoActual;
        [ObservableProperty] private string _nombreCliente = "---";
        [ObservableProperty] private string _fechaVencimientoTexto = "---";
        [ObservableProperty] private string _descripcionPrendas = "---";

        // --- LÓGICA DE COBRO (NUEVO) ---
        // Lista para el ComboBox (1 a 12 meses)
        public List<int> MesesAPagarOpciones { get; } = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        [ObservableProperty]
        private int _mesesAPagarSeleccionados = 1; // Por defecto 1 mes

        [ObservableProperty] private decimal _interesPorMesOriginal = 0; // Calculado fijo

        // --- DESGLOSE FINANCIERO (Reaccionan al combo) ---
        [ObservableProperty] private decimal _capitalPendiente = 0;
        [ObservableProperty] private decimal _interesesAcumulados = 0; // Total de meses seleccionados
        [ObservableProperty] private decimal _totalDesempeno = 0;

        // --- HISTORIAL (NUEVO) ---
        // Colección para la tabla de la derecha
        public ObservableCollection<Pago> HistorialPagos { get; } = new();

        public bool HayContratoActivo => ContratoActual != null && ContratoActual.Estado == EstadoContrato.ACTIVO;

        public CajaViewModel()
        {
            _dbContext = new EmpsysDbContext();
            _cajaService = new CajaService(_dbContext);
            _finanzasService = new FinanzasService();
        }

        // Se ejecuta automáticamente cuando el usuario cambia el ComboBox (Reactivo)
        partial void OnMesesAPagarSeleccionadosChanged(int value)
        {
            ActualizarCalculosFinales();
        }

        private void ActualizarCalculosFinales()
        {
            if (ContratoActual == null) return;

            // Recalculamos basándonos puramente en la selección del combo
            InteresesAcumulados = InteresPorMesOriginal * MesesAPagarSeleccionados;
            TotalDesempeno = CapitalPendiente + InteresesAcumulados;
        }

        [RelayCommand]
        public void BuscarContrato()
        {
            LimpiarPantalla();
            if (!int.TryParse(NumeroContratoBusqueda, out int numContrato)) return;

            var contrato = _dbContext.Contratos
                .Include(c => c.Inventarios).ThenInclude(i => i.Articulo) // Traemos la descripción
                .FirstOrDefault(c => c.NumeroContrato == numContrato);

            if (contrato == null || contrato.Estado != EstadoContrato.ACTIVO) return;

            var cliente = _dbContext.Clientes.FirstOrDefault(c => c.Id == contrato.ClienteId);

            ContratoActual = contrato;
            NombreCliente = cliente?.NombreCompleto ?? "Desconocido";
            FechaVencimientoTexto = contrato.FechaVencimiento.ToString("dd/MM/yyyy");

            // Formateamos la descripción de prendas (Maestro-Detalle)
            DescripcionPrendas = string.Join(", ", contrato.Inventarios.Select(i => i.Articulo.Descripcion));

            CapitalPendiente = contrato.MontoPrestamo;

            // 🧮 LÓGICA LEGACY: Calculamos el interés FIJO por 1 mes
            // (Monto * Tasa / 100)
            InteresPorMesOriginal = contrato.MontoPrestamo * (contrato.TasaInteresMensual / 100);

            MesesAPagarSeleccionados = 1; // Reseteamos el combo a 1
            ActualizarCalculosFinales();

            // 📜 CARGAR HISTORIAL DE RENOVACIONES
            CargarHistorial(numContrato);

            Mensaje = "Contrato cargado.";
            OnPropertyChanged(nameof(HayContratoActivo));
        }

        private void CargarHistorial(int numContrato)
        {
            HistorialPagos.Clear();

            // Buscamos los pagos anteriores de este contrato
            var pagos = _dbContext.Pagos
                .Where(p => p.NumeroContrato == numContrato)
                .OrderByDescending(p => p.FechaPago) // El más reciente arriba
                .ToList();

            foreach (var p in pagos) HistorialPagos.Add(p);
        }

        [RelayCommand]
        public void ProcesarRenovacion()
        {
            try
            {
                // Pasamos los meses seleccionados al servicio
                _cajaService.ProcesarPago(ContratoActual.NumeroContrato, InteresesAcumulados, TipoPago.Intereses, 1, MesesAPagarSeleccionados);

                Mensaje = $"¡Renovación Exitosa por {MesesAPagarSeleccionados} meses!";
                BuscarContrato(); // Recargamos para ver nuevas fechas e historial
            }
            catch (DbUpdateException ex)
            {
                
                Mensaje = $"Error: {ex.Message}";
                Debug.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Mensaje = $"Error: {ex.Message}";
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        [RelayCommand]
        public void ProcesarDesempeno()
        {
            try
            {
                _cajaService.ProcesarPago(ContratoActual.NumeroContrato, TotalDesempeno, TipoPago.Cancelacion, 1);

                Mensaje = $"¡Desempeño Exitoso! Se cobraron ${TotalDesempeno:F2}. Prendas liberadas.";

                ContratoActual = null; // Quitamos el contrato de pantalla porque ya murió
                OnPropertyChanged(nameof(HayContratoActivo));
            }
            catch (Exception ex)
            {
                Mensaje = $"Error al cancelar: {ex.Message}";
            }
        }

        private void LimpiarPantalla()
        {
            ContratoActual = null;
            NombreCliente = "---";
            FechaVencimientoTexto = "---";
            CapitalPendiente = 0;
            InteresesAcumulados = 0;
            TotalDesempeno = 0;
            Mensaje = string.Empty;
            OnPropertyChanged(nameof(HayContratoActivo));
        }
    }
}