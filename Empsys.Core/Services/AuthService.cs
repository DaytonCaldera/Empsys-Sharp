using System.Linq;
using Empsys.Core.Data;
using Empsys.Core.Models;

namespace Empsys.Core.Services
{
    public class AuthService
    {
        private readonly EmpsysDbContext _dbContext;

        public AuthService(EmpsysDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Usuario Autenticar(string username, string password)
        {
            // Busca en SQLite si existe un usuario que coincida exactamente con las credenciales
            return _dbContext.Usuarios.FirstOrDefault(u => u.Username == username && u.Password == password);
        }
    }
}