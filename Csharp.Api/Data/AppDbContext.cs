using Microsoft.EntityFrameworkCore;
using Csharp.Api.Entities;

namespace Csharp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Moto> Motos { get; set; }
        public DbSet<TagBle> TagsBle { get; set; }

        public DbSet<Funcionario> Funcionarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Moto>()
                .HasIndex(m => m.Placa)
                .IsUnique(false);

            modelBuilder.Entity<TagBle>()
                .HasIndex(t => t.CodigoUnicoTag)
                .IsUnique(true);

            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.Email)
                .IsUnique(true);
        }
    }
}
