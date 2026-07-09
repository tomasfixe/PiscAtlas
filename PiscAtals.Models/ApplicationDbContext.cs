using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models.Models;

namespace PiscAtlas.Models
{
    
    public class ApplicationDbContext : IdentityDbContext<Utilizador>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Pesqueiro> Pesqueiros { get; set; }
        public DbSet<Captura> Capturas { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }
        public DbSet<Seguidor> Seguidores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Captura>()
                .HasOne(c => c.Especie)
                .WithMany(e => e.Capturas)
                .HasForeignKey(c => c.EspecieId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Captura>()
                .HasOne(c => c.Pesqueiro)
                .WithMany(p => p.Capturas)
                .HasForeignKey(c => c.PesqueiroId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Seguidor>()
                .HasKey(s => new { s.SeguidorId, s.SeguidoId }); // A chave primária é a junção dos dois IDs

            modelBuilder.Entity<Seguidor>()
                .HasOne(s => s.UtilizadorSeguidor)
                .WithMany(u => u.A_Seguir)
                .HasForeignKey(s => s.SeguidorId)
                .OnDelete(DeleteBehavior.NoAction); // Impede que ao apagar um utilizador, apague a base de dados inteira em cascata

            modelBuilder.Entity<Seguidor>()
                .HasOne(s => s.UtilizadorSeguido)
                .WithMany(u => u.Seguidores)
                .HasForeignKey(s => s.SeguidoId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}