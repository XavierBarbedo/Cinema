using Microsoft.EntityFrameworkCore;
using Cinema.Models;

namespace Cinema.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Filme> Filmes { get; set; }
        public DbSet<Sessao> Sessoes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações de entidades (se houver)...

            // ✅ Seed: Admin inicial
            modelBuilder.Entity<Utilizador>().HasData(
                new Utilizador
                {
                    Id = 1,
                    Nome = "Admin",
                    Email = "cisco@gmail.com",
                    Password = "cisco123", // ⚠️ Só para teste, ideal usar hash
                    Role = "Administrador"
                }
            );
        }
    }
}
