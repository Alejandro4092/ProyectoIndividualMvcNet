using Microsoft.EntityFrameworkCore;
using ProyectoIndividualMvcNet.Models;

namespace ProyectoIndividualMvcNet.Data
{
    public class TiendaJuegosContext : DbContext
    {
        public TiendaJuegosContext() { }

        public TiendaJuegosContext(DbContextOptions<TiendaJuegosContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioSecurity> UsuariosSecurity { get; set; }

        public DbSet<Juego> Juegos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        public DbSet<Resena> Resenas { get; set; }
        public DbSet<UsuarioLogin> VistaUsuariosLogin { get; set; }
        public DbSet<CompraRealizada> ComprasRealizadas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Opcional: Esto ayuda si tienes problemas con los nombres en mayúsculas
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<UsuarioSecurity>().ToTable("Usuarios_Security");

        }
    }
}