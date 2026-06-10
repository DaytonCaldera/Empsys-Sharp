using System;
using Empsys.Core.Models;
using Empsys.Core.Services;
using Empsys.Core.Data; // <-- 1. Agregamos esta referencia
using Xunit;

namespace Empsys.Tests
{
    public class ClienteServiceTests
    {
        private readonly ClienteService _service;

        public ClienteServiceTests()
        {
            // 2. Instanciamos el contexto y se lo inyectamos al servicio
            var dbContext = new EmpsysDbContext();
            _service = new ClienteService(dbContext);
        }

        [Fact]
        public void ValidarElegibilidad_DeberiaRetornarTrue_CuandoElClienteEsConfiable()
        {
            // ARRANGED
            var clienteNormal = new Cliente
            {
                Cedula = "12345678",
                NivelRiesgo = NivelRiesgoCliente.Confiable
            };

            // ACT
            bool esElegible = _service.ValidarElegibilidadCliente(clienteNormal);

            // ASSERT
            Assert.True(esElegible);
        }

        [Fact]
        public void ValidarElegibilidad_DeberiaRetornarFalse_CuandoElClienteEsProblematico()
        {
            // ARRANGED: Replicamos el antiguo "problemas = 'S'"
            var clienteMoroso = new Cliente
            {
                Cedula = "87654321",
                NivelRiesgo = NivelRiesgoCliente.Problematico
            };

            // ACT
            bool esElegible = _service.ValidarElegibilidadCliente(clienteMoroso);

            // ASSERT: Protegemos al negocio de prestarle dinero
            Assert.False(esElegible);
        }

        [Fact]
        public void MarcarComoProblematico_DeberiaCambiarElEstadoDelCliente()
        {
            // ARRANGED
            var cliente = new Cliente { Cedula = "111", NivelRiesgo = NivelRiesgoCliente.Confiable };

            // ACT
            _service.MarcarComoProblematico(cliente, "Abandono de contrato sin pago");

            // ASSERT
            Assert.Equal(NivelRiesgoCliente.Problematico, cliente.NivelRiesgo);
            Assert.False(cliente.EsElegibleParaPrestamo);
        }
    }
}