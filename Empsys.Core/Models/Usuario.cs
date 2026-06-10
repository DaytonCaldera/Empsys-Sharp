namespace Empsys.Core.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;

        // Nota Senior: Por simplicidad ahora usaremos texto plano, 
        // pero antes de ir a producción, esto debe ser un Hash (ej. SHA256).
        public string Password { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
    }
}