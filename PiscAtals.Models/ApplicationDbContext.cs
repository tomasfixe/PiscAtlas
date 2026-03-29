using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace PiscAtlas.Models
{
    public class ApplicationDbContext : DbContext
    {
        // Construtor obrigatório para passarmos a configuração mais tarde
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Declaração as nossas 6 tabelas
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Pesqueiro> Pesqueiros { get; set; }
        public DbSet<Captura> Capturas { get; set; }
        public DbSet<Denuncia> Denuncias { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }

        // Esta função previne que apagar uma espécie apague acidentalmente todas as capturas associadas a ela
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
        }
    }
}
