using System;
using System.Linq;
using Empsys.Core.Data;
using Empsys.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Empsys.Core.Services
{
    public class CajaService
    {
        private readonly IEmpsysDbContext _dbContext;

        public CajaService(IEmpsysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Procesa un movimiento de caja, registrando el pago y aplicando 
        /// las reglas de negocio al contrato y su inventario de forma atómica.
        /// </summary>
        public Pago ProcesarPago(int contratoId, decimal montoPagado, TipoPago tipoPago, int usuarioId, int mesesPagados = 1)
        {
            // 1. Validaciones iniciales
            if (montoPagado <= 0)
                throw new ArgumentException("El monto del pago debe ser mayor a cero.");

            var contrato = _dbContext.Contratos
                .Local
                .FirstOrDefault(c => c.NumeroContrato == contratoId)
                ?? _dbContext.Contratos
                    .Include(c => c.Inventarios)
                    .FirstOrDefault(c => c.NumeroContrato == contratoId);

            if (contrato == null)
                throw new InvalidOperationException("El contrato no existe.");

            if (contrato.Estado != EstadoContrato.ACTIVO)
                throw new InvalidOperationException($"No se puede cobrar un contrato en estado: {contrato.Estado}.");

            // 3. Generamos el registro de auditoría (Recibo)
            var nuevoPago = new Pago
            {
                NumeroContrato = contrato.NumeroContrato,
                Monto = montoPagado,
                FechaPago = DateTime.Now,
                Tipo = tipoPago,
                UsuarioId = usuarioId,
                MesesPagados = mesesPagados // Registramos cuántos meses pagó
            };

            // 4. Aplicamos el Motor de Reglas de Negocio según el tipo de pago
            switch (tipoPago)
            {
                case TipoPago.Intereses:
                    // MODO RENOVACIÓN (Refrendo)
                    DateTime fechaBase = contrato.FechaVencimiento > DateTime.Now.Date ? contrato.FechaVencimiento : DateTime.Now.Date;
                    DateTime nuevaVencimiento = fechaBase.AddDays(mesesPagados * 30);
                    // El cliente paga los intereses y su plazo se reinicia a partir de hoy
                    contrato.FechaVencimiento = nuevaVencimiento;
                    // Reiniciamos la fecha de inicio para el próximo ciclo de cálculo
                    contrato.FechaInicio = DateTime.Now.Date;
                    // Los artículos siguen en estado EMPENIADO, así que no los tocamos.
                    nuevoPago.NuevaFechaVencimiento = nuevaVencimiento;
                    break;

                case TipoPago.Cancelacion:
                    // MODO DESEMPEÑO
                    // El cliente liquida la deuda. El contrato muere.
                    contrato.Estado = EstadoContrato.CANCELADO;

                    // Liberamos las prendas de la bóveda
                    foreach (var itemInventario in contrato.Inventarios)
                    {
                        // Nota Senior: Como tu enum actual solo tiene INVENTARIO, EMPENIADO, VENDIDO
                        // Usaremos VENDIDO para indicar que ya salió de la casa de empeños. 
                        // (Te sugiero agregar 'DEVUELTO' a tu Enum EstadoArticulo en el futuro).
                        itemInventario.Estado = EstadoArticulo.VENDIDO;
                    }
                    break;

                case TipoPago.AbonoCapital:
                    // MODO ABONO PARCIAL
                    // Reduce la deuda principal, lo que hará que el próximo cálculo de intereses sea menor.
                    contrato.MontoPrestamo -= montoPagado;
                    if (contrato.MontoPrestamo < 0)
                        contrato.MontoPrestamo = 0;
                    break;
            }

            // 5. Transacción Atómica: Guardamos el Recibo + Actualización de Contrato + Cambio de Inventarios
            // O se guarda todo, o no se guarda nada (Cero datos corruptos).
            _dbContext.Pagos.Add(nuevoPago);
            _dbContext.SaveChanges();

            return nuevoPago;
        }
    }
}