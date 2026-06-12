using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Empsys.Core.Models;
using Empsys.Core.Services;
using Empsys.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Empsys.Tests
{
    public class CajaServiceTests
    {
        private readonly CajaService _service;
        private readonly List<Contrato> _contratosData;
        private readonly List<Pago> _pagosData;

        public CajaServiceTests()
        {
            // 1. Inicializamos nuestras bases de datos en memoria (Listas)
            _contratosData = GenerarContratosMock();
            _pagosData = new List<Pago>();

            // 2. Creamos los Mocks de los DbSets
            var mockContratos = MockDbSet(_contratosData);
            var mockPagos = MockDbSet(_pagosData);

            // 3. Mockeamos el Contexto de la Base de Datos
            // Nota: Si usas una interfaz como IEmpsysDbContext en tu proyecto, cámbialo aquí.
            // Asumo EmpsysDbContext con propiedades virtual o IEmpsysDbContext según tu estándar.
            var mockContext = new Mock<IEmpsysDbContext>();
            mockContext.Setup(c => c.Contratos).Returns(mockContratos.Object);
            mockContext.Setup(c => c.Pagos).Returns(mockPagos.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            // 4. Inyectamos el contexto falso al servicio real
            _service = new CajaService(mockContext.Object);
        }

        [Fact]
        public void ProcesarPago_DeberiaLanzarExcepcion_CuandoMontoEsCeroONegativo()
        {
            // ACT & ASSERT
            var excepcion = Assert.Throws<ArgumentException>(() =>
                _service.ProcesarPago(1001, 0, TipoPago.Intereses, 1, 1)
            );

            Assert.Equal("El monto del pago debe ser mayor a cero.", excepcion.Message);
        }

        [Fact]
        public void ProcesarPago_DeberiaLanzarExcepcion_CuandoContratoNoEsActivo()
        {
            // ARRANGED - Intentamos cobrarle a un contrato ya cancelado (1002)

            // ACT & ASSERT
            var excepcion = Assert.Throws<InvalidOperationException>(() =>
                _service.ProcesarPago(1002, 500, TipoPago.Cancelacion, 1)
            );

            Assert.Contains("No se puede cobrar un contrato en estado", excepcion.Message);
        }

        [Fact]
        public void ProcesarRenovacion_DeberiaActualizarFechasYRegistrarPago_CuandoEsPagoDeIntereses()
        {
            // ARRANGED
            int contratoId = 1001; // Contrato Activo
            decimal montoCobrado = 1500;
            int mesesAPagar = 2; // Paga 2 meses por adelantado
            var contrato = _contratosData.First(c => c.NumeroContrato == contratoId);
            DateTime fechaVencimientoAnterior = contrato.FechaVencimiento;

            // ACT
            var reciboPago = _service.ProcesarPago(contratoId, montoCobrado, TipoPago.Intereses, 1, mesesAPagar);

            // ASSERT - Verificaciones del motor financiero
            Assert.NotNull(reciboPago);
            Assert.Equal(montoCobrado, reciboPago.Monto);
            Assert.Equal(TipoPago.Intereses, reciboPago.Tipo);
            Assert.Equal(mesesAPagar, reciboPago.MesesPagados);

            // Verificamos que el contrato se haya extendido comercialmente (2 meses = 60 días)
            Assert.Equal(fechaVencimientoAnterior.AddDays(60), contrato.FechaVencimiento);
            Assert.Equal(DateTime.Now.Date, contrato.FechaInicio); // Reseteo de cálculo

            // Verificamos que el recibo guardó la nueva fecha para el historial
            Assert.Equal(contrato.FechaVencimiento, reciboPago.NuevaFechaVencimiento);

            // Verificamos que el pago se agregó a la tabla "falsa"
            Assert.Single(_pagosData);
        }

        [Fact]
        public void ProcesarCancelacion_DeberiaCambiarEstadoDeContratoYPrendas_CuandoEsDesempeno()
        {
            // ARRANGED
            int contratoId = 1001; // Contrato Activo
            var contrato = _contratosData.First(c => c.NumeroContrato == contratoId);

            // ACT
            var reciboPago = _service.ProcesarPago(contratoId, 5000, TipoPago.Cancelacion, 1);

            // ASSERT - Consecuencias del Desempeño
            Assert.Equal(EstadoContrato.CANCELADO, contrato.Estado);

            // Fundamental: Verificar que la transacción atómica liberó el inventario
            Assert.All(contrato.Inventarios, articulo =>
                Assert.Equal(EstadoArticulo.VENDIDO, articulo.Estado)
            );

            Assert.Equal(TipoPago.Cancelacion, reciboPago.Tipo);
        }

        [Fact]
        public void ProcesarAbono_DeberiaReducirCapitalPendiente_CuandoEsAbonoCapital()
        {
            // ARRANGED
            int contratoId = 1001;
            var contrato = _contratosData.First(c => c.NumeroContrato == contratoId);
            decimal capitalInicial = contrato.MontoPrestamo;
            decimal abono = 2000;

            // ACT
            _service.ProcesarPago(contratoId, abono, TipoPago.AbonoCapital, 1);

            // ASSERT
            Assert.Equal(capitalInicial - abono, contrato.MontoPrestamo);
        }

        // --- MÉTODOS HELPER ---

        /// <summary>
        /// Genera la data falsa (semilla) para las pruebas
        /// </summary>
        private List<Contrato> GenerarContratosMock()
        {
            return new List<Contrato>
            {
                // Contrato 1: Sano y Activo
                new Contrato
                {
                    NumeroContrato = 1001,
                    Estado = EstadoContrato.ACTIVO,
                    MontoPrestamo = 5000,
                    FechaInicio = DateTime.Now.AddDays(-15),
                    FechaVencimiento = DateTime.Now.AddDays(15),
                    Inventarios = new List<Inventario>
                    {
                        new Inventario { Id = 1, Estado = EstadoArticulo.EMPENIADO },
                        new Inventario { Id = 2, Estado = EstadoArticulo.EMPENIADO }
                    }
                },
                // Contrato 2: Ya cancelado (Para probar validaciones de error)
                new Contrato
                {
                    NumeroContrato = 1002,
                    Estado = EstadoContrato.CANCELADO,
                    MontoPrestamo = 3000,
                    Inventarios = new List<Inventario>()
                }
            };
        }

        /// <summary>
        /// Reusable helper para mockear DbSets interceptando el método '.Add'
        /// </summary>
        private static Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mock = new Mock<DbSet<T>>();

            mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // 🔥 ¡CLAVE!: Configurar el Mock para que cuando el servicio llame a .Add(), 
            // realmente inserte el objeto en nuestra lista en memoria para poder contarlo en los Asserts.
            mock.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);

            return mock;
        }
    }
}