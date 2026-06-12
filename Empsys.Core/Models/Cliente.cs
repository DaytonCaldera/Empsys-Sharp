namespace Empsys.Core.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido1 { get; set; } = string.Empty;
        public string Apellido2 { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        // Usamos el Enum tipado
        public NivelRiesgoCliente NivelRiesgo { get; set; } = NivelRiesgoCliente.Confiable;

        // Propiedad calculada: lógica pura de dominio
        public bool EsElegibleParaPrestamo => NivelRiesgo == NivelRiesgoCliente.Confiable;
    }
}