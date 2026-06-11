using Empsys.Core.Data;
using Empsys.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Empsys.Core.Services
{
    public class ContratoService
    {
        private readonly FinanzasService _finanzasService;
        private readonly EmpsysDbContext _dbContext;

        public ContratoService(EmpsysDbContext dbContext)
        {
            _dbContext = dbContext;
            _finanzasService = new FinanzasService();
        }

        public Contrato CrearNuevoContrato(Cliente cliente, List<Inventario> articulosEnGrid, string descripcionContrato, decimal tasaInteres)
        {
            if (cliente == null || !cliente.EsElegibleParaPrestamo)
                throw new InvalidOperationException("Cliente no válido o con riesgo problemático.");

            if (articulosEnGrid == null || articulosEnGrid.Count == 0)
                throw new InvalidOperationException("Debe incluir al menos un artículo en la tabla.");

            // El monto total se calcula matemáticamente sumando el grid (a prueba de errores humanos)
            decimal montoTotalCalculado = articulosEnGrid.Sum(a => a.PrecioEstimado);

            var nuevoContrato = new Contrato
            {
                ClienteId = cliente.Id,
                Descripcion = descripcionContrato,
                MontoPrestamo = montoTotalCalculado,
                TasaInteresMensual = tasaInteres,
                FechaInicio = DateTime.Now.Date,
                FechaVencimiento = DateTime.Now.Date.AddDays(30),
                Estado = EstadoContrato.ACTIVO,
                Inventarios = new List<Inventario>()
            };

            // Procesamos cada artículo del grid
            foreach (var articulo in articulosEnGrid)
            {
                articulo.Estado = EstadoArticulo.EMPENIADO;
                nuevoContrato.Inventarios.Add(articulo);

                // Como los estamos creando al vuelo, los agregamos a la BD
                _dbContext.Inventarios.Add(articulo);
            }

            _dbContext.Contratos.Add(nuevoContrato);

            // Transacción atómica: Guarda el contrato y todos los artículos en un solo paso
            _dbContext.SaveChanges();

            return nuevoContrato;
        }

        /// <summary>
        /// Mantiene la esencia de 'anula' usando tipado fuerte (Enums).
        /// </summary>
        public bool AnularContrato(Contrato contrato, List<Inventario> articulosDelContrato)
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
        public decimal ProcesarPagoContrato(Contrato contrato, List<Inventario> articulos, DateTime fechaOperacion, bool esRenovacion)
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