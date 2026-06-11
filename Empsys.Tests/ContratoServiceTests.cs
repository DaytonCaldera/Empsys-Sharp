using Empsys.Core.Data;
using Empsys.Core.Models;
using Empsys.Core.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace Empsys.Tests
{
    public class ContratoServiceTests
    {
        private readonly ContratoService _service;

        public ContratoServiceTests()
        {
            var dbContext = new EmpsysDbContext();
            _service = new ContratoService(dbContext);
        }

        [Fact]
        public void AnularContrato_DeberiaCambiarEstadosCorrectamente_CuandoElContratoEsValido()
        {
            // ARRANGED
            var contrato = new Contrato
            {
                NumeroContrato = 5050,
                Estado = EstadoContrato.ACTIVO,
                MontoPrestamo = 5000
            };

            var listaArticulos = new List<Inventario>
            {
                new Inventario { Id = 1, NumeroContrato = 5050, Estado = EstadoArticulo.EMPENIADO },
                new Inventario { Id = 2, NumeroContrato = 5050, Estado = EstadoArticulo.EMPENIADO},
                new Inventario { Id = 3, NumeroContrato = 9999, Estado = EstadoArticulo.EMPENIADO}
            };

            // ACT
            bool resultado = _service.AnularContrato(contrato, listaArticulos);

            // ASSERT
            Assert.True(resultado);
            Assert.Equal(EstadoContrato.ANULADO, contrato.Estado);
            Assert.Equal(EstadoArticulo.INVENTARIO, listaArticulos[0].Estado);
            Assert.Equal(EstadoArticulo.INVENTARIO, listaArticulos[1].Estado);
            Assert.Equal(EstadoArticulo.EMPENIADO, listaArticulos[2].Estado);
        }

        [Fact]
        public void AnularContrato_NoDeberiaPermitirAnulacion_SiElContratoYaEstaCANCELADO()
        {
            // ARRANGED
            var contrato = new Contrato { NumeroContrato = 1010, Estado = EstadoContrato.CANCELADO };
            var articulos = new List<Inventario>();

            // ACT
            bool resultado = _service.AnularContrato(contrato, articulos);

            // ASSERT
            Assert.False(resultado);
            Assert.Equal(EstadoContrato.CANCELADO, contrato.Estado);
        }

        [Fact]
        public void ProcesarPagoContrato_DeberiaCalcularSoloInteresYExtenderFechas_CuandoEsRenovacion()
        {
            // ARRANGED: Un préstamo de $1,000 al 10% mensual creado hace exactamente 30 días
            var fechaInicial = new DateTime(2026, 5, 10);
            var fechaHoy = new DateTime(2026, 6, 9); // 30 días después

            var contrato = new Contrato
            {
                NumeroContrato = 7001,
                MontoPrestamo = 1000,
                TasaInteresMensual = 10,
                FechaInicio = fechaInicial,
                FechaVencimiento = fechaInicial.AddDays(30),
                Estado = EstadoContrato.ACTIVO
            };

            var articulos = new List<Inventario>
            {
                new Inventario { Id = 10, NumeroContrato = 7001, Estado = EstadoArticulo.EMPENIADO }
            };

            // ACT: El cliente renueva el contrato hoy
            decimal montoCobrado = _service.ProcesarPagoContrato(contrato, articulos, fechaHoy, esRenovacion: true);

            // ASSERT: Debe cobrar el 10% de interés ($100) sin tocar el capital
            Assert.Equal(100.00m, montoCobrado);
            Assert.Equal(EstadoContrato.ACTIVO, contrato.Estado); // Sigue activo
            Assert.Equal(fechaHoy, contrato.FechaInicio); // La nueva fecha de inicio es hoy
            Assert.Equal(fechaHoy.AddDays(30), contrato.FechaVencimiento); // Vence en un mes comercial
            Assert.Equal(EstadoArticulo.EMPENIADO, articulos[0].Estado); // El artículo sigue retenido
        }

        [Fact]
        public void ProcesarPagoContrato_DeberiaCobrarTotalYLiberarArticulos_CuandoEsCancelacion()
        {
            // ARRANGED: Un préstamo de $2,000 al 5% mensual creado hace 30 días
            var fechaInicial = new DateTime(2026, 5, 10);
            var fechaHoy = new DateTime(2026, 6, 9);

            var contrato = new Contrato
            {
                NumeroContrato = 7002,
                MontoPrestamo = 2000,
                TasaInteresMensual = 5,
                FechaInicio = fechaInicial,
                FechaVencimiento = fechaInicial.AddDays(30),
                Estado = EstadoContrato.ACTIVO
            };

            var articulos = new List<Inventario>
            {
                new Inventario { Id = 11, NumeroContrato = 7002, Estado = EstadoArticulo.EMPENIADO }
            };

            // ACT: El cliente cancela y retira sus prendas hoy
            decimal montoCobrado = _service.ProcesarPagoContrato(contrato, articulos, fechaHoy, esRenovacion: false);

            // ASSERT: Debe cobrar Capital ($2000) + Interés ($100) = $2100
            Assert.Equal(2100.00m, montoCobrado);
            Assert.Equal(EstadoContrato.CANCELADO, contrato.Estado); // Contrato cerrado
            Assert.Equal(EstadoArticulo.VENDIDO, articulos[0].Estado); // Artículo liberado
        }

        [Fact]
        public void RevertirOperacion_DeberiaRestaurarEstadosOriginales_CuandoSeSolicita()
        {
            // ARRANGED
            var contrato = new Contrato { NumeroContrato = 8000 };
            var fechaInicOriginal = new DateTime(2026, 1, 1);
            var fechaVencOriginal = new DateTime(2026, 2, 1);

            // ACT
            _service.RevertirOperacion(contrato, fechaInicOriginal, fechaVencOriginal, EstadoContrato.ACTIVO);

            // ASSERT
            Assert.Equal(fechaInicOriginal, contrato.FechaInicio);
            Assert.Equal(fechaVencOriginal, contrato.FechaVencimiento);
            Assert.Equal(EstadoContrato.ACTIVO, contrato.Estado);
        }
    }
}