using Empsys.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empsys.Core.Data
{
    public interface IEmpsysDbContext
    {
        DbSet<Contrato> Contratos { get; }
        DbSet<Inventario> Inventarios { get; }
        DbSet<Pago> Pagos { get; }
        int SaveChanges();
    }
}
