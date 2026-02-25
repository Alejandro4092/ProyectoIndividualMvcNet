using Microsoft.EntityFrameworkCore;
using TiendaJuegos.Models;

namespace TiendaJuegos.Data
{
    public class TiendaJuegosContext : DbContext
    {
        public TiendaJuegosContext(DbContextOptions<TiendaJuegosContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Juego> Juegos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        public DbSet<Resena> Resenas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Rol).HasDefaultValue("Usuario");
                entity.Property(e => e.FechaAlta).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Juego>(entity =>
            {
                entity.Property(e => e.Stock).HasDefaultValue(0);
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.Property(e => e.Fecha).HasDefaultValueSql("GETDATE()");

                entity.HasOne(p => p.Usuario)
                    .WithMany(u => u.Pedidos)
                    .HasForeignKey(p => p.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.HasOne(pd => pd.Pedido)
                    .WithMany(p => p.PedidoDetalles)
                    .HasForeignKey(pd => pd.PedidoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pd => pd.Juego)
                    .WithMany(j => j.PedidoDetalles)
                    .HasForeignKey(pd => pd.JuegoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Resena>(entity =>
            {
                entity.Property(e => e.Fecha).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => new { e.UsuarioId, e.JuegoId })
                    .IsUnique()
                    .HasDatabaseName("UQ_Usuario_Juego");

                entity.HasOne(r => r.Usuario)
                    .WithMany(u => u.Resenas)
                    .HasForeignKey(r => r.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Juego)
                    .WithMany(j => j.Resenas)
                    .HasForeignKey(r => r.JuegoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}