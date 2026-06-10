using Empsys.Core.Data;
using Empsys.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Empsys.Core.Services
{
    public class ClienteService
    {

        private readonly EmpsysDbContext _dbContext;

        public ClienteService(EmpsysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Mantiene la esencia de 'desplegar_cliente' y validación de problemas, 
        /// pero orientado a objetos y sin tocar la base de datos directamente.
        /// </summary>
        public bool ValidarElegibilidadCliente(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente), "El cliente no existe en los registros.");

            if (string.IsNullOrWhiteSpace(cliente.Cedula))
                throw new ArgumentException("La cédula del cliente es inválida.");

            // Retorna si el cliente es apto para un nuevo contrato basado en su riesgo
            return cliente.EsElegibleParaPrestamo;
        }

        /// <summary>
        /// Permite marcar a un cliente como problemático si, por ejemplo,
        /// abandonó un contrato o no pagó.
        /// </summary>
        public void MarcarComoProblematico(Cliente cliente, string motivo)
        {
            // Podríamos guardar el motivo en un historial, pero por ahora cambiamos el estado
            cliente.NivelRiesgo = NivelRiesgoCliente.Problematico;
        }

        public Cliente BuscarPorCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;

            // FirstOrDefault devuelve el cliente si lo encuentra, o 'null' si no existe
            return _dbContext.Clientes.FirstOrDefault(c => c.Cedula == cedula);
        }

        // Método inteligente: Si existe lo actualiza, si no, lo crea.
        public void GuardarOActualizar(Cliente datosCliente)
        {
            if (string.IsNullOrWhiteSpace(datosCliente.Cedula) || string.IsNullOrWhiteSpace(datosCliente.NombreCompleto))
                throw new ArgumentException("La cédula y el nombre completo son obligatorios.");

            // Verificamos si el cliente ya existe en la base de datos
            var clienteExistente = BuscarPorCedula(datosCliente.Cedula);

            if (clienteExistente != null)
            {
                // MODO EDICIÓN: Actualizamos las propiedades del cliente rastreado
                clienteExistente.NombreCompleto = datosCliente.NombreCompleto;
                clienteExistente.NivelRiesgo = datosCliente.NivelRiesgo;
                _dbContext.Clientes.Update(clienteExistente);
            }
            else
            {
                // MODO INSERCIÓN: Es un cliente nuevo
                _dbContext.Clientes.Add(datosCliente);
            }

            // Guardamos los cambios en SQLite de forma segura y liberamos la tabla de inmediato
            _dbContext.SaveChanges();
        }
    }
}