using Microsoft.EntityFrameworkCore;
using Empsys.Core.Models;
using System;
using System.IO;
using System.Diagnostics;

namespace Empsys.Core.Data
{
    public class EmpsysDbContext : DbContext, IEmpsysDbContext
    {
        // Estas propiedades representan las tablas reales en la base de datos
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Familia> Familias { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Pago> Pagos { get; set; }


        public EmpsysDbContext()
        {
            // Constructor vacío necesario para migraciones de EF Core
        }

        // Si usas inyección de dependencias en WinUI 3, usarás este constructor
        public EmpsysDbContext(DbContextOptions<EmpsysDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            if (!optionsBuilder.IsConfigured)
            {
                // Definimos que sea un archivo local, portátil y liviano.
                // Quedará guardado en la carpeta del usuario (AppData) para evitar problemas de permisos en Windows 10/11
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string path = Path.Combine(folder, "empsys_data.db");
                Debug.WriteLine($"Data Source={path}");
                optionsBuilder.UseSqlite($"Data Source={path}").EnableSensitiveDataLogging()
                  .LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                         Microsoft.Extensions.Logging.LogLevel.Information);

            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aquí podemos darle instrucciones específicas a SQLite sin ensuciar nuestros modelos

            // Ejemplo: Asegurarnos de que la Cédula sea única y rápida de buscar (Índice)
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Cedula)
                .IsUnique();
            modelBuilder.Entity<Contrato>()
                .HasKey(c => c.NumeroContrato);

            // Configurar los Enums para que se guarden como texto legible en la base de datos (ej: "Activo" en lugar de "0")
            modelBuilder.Entity<Contrato>()
                .Property(c => c.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Inventario>()
                .Property(i => i.Estado)
                .HasConversion<string>();

            modelBuilder.Entity<Cliente>()
                .Property(c => c.NivelRiesgo)
                .HasConversion<string>();

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Username = "admin",
                    Password = "admin", // Contraseña por defecto
                    NombreCompleto = "Administrador del Sistema"
                }
            );
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Contrato)
                .WithMany()
                .HasForeignKey(p => p.NumeroContrato);

            modelBuilder.Entity<Pago>()
                .Property(p => p.Tipo)
                .HasConversion<string>();
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Contrato)
                .WithMany(c => c.Inventarios)
                .HasForeignKey(i => i.NumeroContrato);
        }
    }
}