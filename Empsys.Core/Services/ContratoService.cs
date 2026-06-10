using System;
using System.Collections.Generic;
using System.Linq;
using Empsys.Core.Models;

namespace Empsys.Core.Services
{
    public class ContratoService
    {
        private readonly FinanzasService _finanzasService;

        public ContratoService()
        {
            _finanzasService = new FinanzasService();
        }

        /// <summary>
        /// Mantiene la esencia de 'anula' usando tipado fuerte (Enums).
        /// </summary>
        public bool AnularContrato(Contrato contrato, List<Articulo> articulosDelContrato)
        {
            if (contrato.Estado == EstadoContrato.CANCELADO || contrato.Estado == EstadoContrato.ANULADO)
            {
                return false;
            }

            contrato.Estado = EstadoContrato.ANULADO;

            var articulosAfectados = articulosDelContrato.Where(a => a.NumeroContrato == contrato.NumeroContrato);
            foreach (var articulo in articulosAfectados)
            {
                articulo.Estado = EstadoArticulo.INVENTARIO;
            }

            return true;
        }

        /// <summary>
        /// Refactorización de la esencia de 'cancela_contrato'. 
        /// Procesa la liquidación total o la renovación (extensión) de un contrato.
        /// </summary>
        /// <param name="contrato">El contrato a procesar.</param>
        /// <param name="articulos">Lista global de artículos.</param>
        /// <param name="fechaOperacion">Fecha en la que el cliente llega a la sucursal.</param>
        /// <param name="esRenovacion">True si solo paga intereses para extender; False si cancela totalmente.</param>
        /// <returns>El monto total que el cliente debe pagar en caja.</returns>
        public decimal ProcesarPagoContrato(Contrato contrato, List<Articulo> articulos, DateTime fechaOperacion, bool esRenovacion)
        {
            if (contrato.Estado != EstadoContrato.ACTIVO)
                throw new InvalidOperationException("El contrato no se encuentra activo para procesar pagos.");

            // 1. Calcular interés acumulado a la fecha de la operación usando el servicio financiero refinado
            decimal interesesAcumulados = _finanzasService.CalcularIntereses(contrato, fechaOperacion);
            decimal montoCobro = 0;

            if (esRenovacion)
            {
                // Esencia de Renovación: Solo paga intereses. 
                montoCobro = interesesAcumulados;

                // Corrección de Bug Lógico: En Pascal las fechas mutaban de forma descontrolada.
                // Aquí establecemos explícitamente el nuevo ciclo de vida del contrato (30 días comerciales a partir de hoy)
                contrato.FechaInicio = fechaOperacion;
                contrato.FechaVencimiento = fechaOperacion.AddDays(30);
                contrato.Estado = EstadoContrato.ACTIVO; // Sigue activo para el siguiente mes
            }
            else
            {
                // Esencia de Cancelación: Paga el préstamo completo (Capital) + los intereses
                montoCobro = contrato.MontoPrestamo + interesesAcumulados;

                // El contrato muere legítimamente
                contrato.Estado = EstadoContrato.CANCELADO;

                // Los artículos asociados se liberan y regresan con el dueño (ya no están empeñados ni en inventario)
                var articulosAsociados = articulos.Where(a => a.NumeroContrato == contrato.NumeroContrato);
                foreach (var articulo in articulosAsociados)
                {
                    articulo.Estado = EstadoArticulo.VENDIDO; // O un estado específico como 'DevueltoAlCliente' si decides expandir el Enum
                }
            }

            return Math.Round(montoCobro, 2);
        }

        /// <summary>
        /// Mantiene la esencia de 'restore_contract_date'.
        /// Permite revertir una renovación errónea en caja, restaurando los valores de tiempo originales.
        /// </summary>
        public void RevertirOperacion(Contrato contrato, DateTime fechaInicioOriginal, DateTime fechaVencimientoOriginal, EstadoContrato estadoOriginal)
        {
            // Restaura de manera segura los estados y tiempos sin depender de cursores de BD corruptos
            contrato.FechaInicio = fechaInicioOriginal;
            contrato.FechaVencimiento = fechaVencimientoOriginal;
            contrato.Estado = estadoOriginal;
        }
    }
}