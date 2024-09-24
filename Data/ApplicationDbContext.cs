using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokeTorneio.Models;

namespace PokeTorneio.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Rodada> Rodadas { get; set; }
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<Torneio> Torneios { get; set; }
        public DbSet<Jogador> Jogadores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações para a relação entre Partida e Jogador1
            modelBuilder.Entity<Partida>()
                .HasOne(p => p.Jogador1)
                .WithMany(j => j.Partidas)
                .HasForeignKey(p => p.Jogador1Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações para a relação entre Partida e Jogador2
            modelBuilder.Entity<Partida>()
                .HasOne(p => p.Jogador2)
                .WithMany() // Pode ser melhor ter uma coleção em Jogador, mas depende do seu modelo
                .HasForeignKey(p => p.Jogador2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configurações para a relação entre Rodada e Partida
            modelBuilder.Entity<Rodada>()
                .HasMany(r => r.Partidas)
                .WithOne(p => p.Rodada)
                .HasForeignKey(p => p.RodadaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}