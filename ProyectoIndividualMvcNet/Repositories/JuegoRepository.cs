using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Models;

namespace ProyectoIndividualMvcNet.Repositories
{
    public class JuegoRepository
    {
        private TiendaJuegosContext context;

        public JuegoRepository(TiendaJuegosContext context)
        {
            this.context = context;
        }
        public async Task<List<Juego>> GetJuegosAsync()
        {
           
            var consulta = from datos in this.context.Juegos
                           where datos.Activo == true
                           orderby datos.Titulo ascending
                           select datos;

            return await consulta.ToListAsync();
        }
        public async Task<Juego> FindJuegoAsync(int idJuego)
        {
            var consulta = from datos in this.context.Juegos where datos.Id == idJuego select datos;
            return await consulta.FirstOrDefaultAsync();
        }
        public async Task<List<Juego>> GetJuegosPlataformaGeneroAsync(string? texto, string? genero, string? plataforma)
        {
            texto ??= "";
            genero ??= "";
            plataforma ??= "";

            string sql = "sp_BuscarJuegos @Texto, @Genero, @Plataforma";

            SqlParameter paramTexto = new SqlParameter("@Texto", texto);
            SqlParameter paramGenero = new SqlParameter("@Genero", genero);
            SqlParameter paramPlataforma = new SqlParameter("@Plataforma", plataforma);

            var consulta = await this.context.Juegos
                 .FromSqlRaw(sql, paramTexto, paramGenero, paramPlataforma)
                 .ToListAsync();

            return consulta;
        }
        public async Task InsertJuegoAsync(string titulo, string descripcion, decimal precio, 
            int stock, string genero, string plataforma, string img, bool activo)
        {
            string sql = "sp_InsertarJuego @Titulo, @Descripcion, @Precio, @Stock, @Genero, @Plataforma, @Img, @Activo";

            SqlParameter pamTitulo = new SqlParameter("@Titulo", titulo);
            SqlParameter pamDescripcion = new SqlParameter("@Descripcion", descripcion);
            SqlParameter pamPrecio = new SqlParameter("@Precio", precio);
            SqlParameter pamStock = new SqlParameter("@Stock", stock);
            SqlParameter pamGenero = new SqlParameter("@Genero", genero);
            SqlParameter pamPlataforma = new SqlParameter("@Plataforma", plataforma);
            SqlParameter pamImg = new SqlParameter("@Img", img);
            SqlParameter pamActivo = new SqlParameter("@Activo", activo);

            await this.context.Database.ExecuteSqlRawAsync(sql, 
                pamTitulo, pamDescripcion, pamPrecio, pamStock, 
                pamGenero, pamPlataforma, pamImg, pamActivo);
        }
        public async Task EliminarJuegoAsync(int id)
        {
            string sql = "sp_EliminarJuego @id";

            SqlParameter pamId = new SqlParameter("@id", id);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamId);
        }
        public async Task EditJuegoAsync(int id, string titulo, string descripcion, decimal precio,
            int stock, string genero, string plataforma, string img, bool activo)
        {
            string sql = "sp_EditarJuego @Id, @Titulo, @Descripcion, @Precio, @Stock, @Genero, @Plataforma, @Img, @Activo";

            SqlParameter pamId = new SqlParameter("@Id", id);
            SqlParameter pamTitulo = new SqlParameter("@Titulo", titulo);
            SqlParameter pamDescripcion = new SqlParameter("@Descripcion", descripcion);
            SqlParameter pamPrecio = new SqlParameter("@Precio", precio);
            SqlParameter pamStock = new SqlParameter("@Stock", stock);
            SqlParameter pamGenero = new SqlParameter("@Genero", genero);
            SqlParameter pamPlataforma = new SqlParameter("@Plataforma", plataforma);
            SqlParameter pamImg = new SqlParameter("@Img", img);
            SqlParameter pamActivo = new SqlParameter("@Activo", activo);

            await this.context.Database.ExecuteSqlRawAsync(sql,
                pamId, pamTitulo, pamDescripcion, pamPrecio, pamStock,
                pamGenero, pamPlataforma, pamImg, pamActivo);
        }
        public async Task ComprarJuegoAsync(int idUsuario, int idJuego, decimal precio)
        {
     
            Juego juego = await this.context.Juegos.FirstOrDefaultAsync(x => x.Id == idJuego);

            if (juego != null && juego.Stock > 0)
            {
                juego.Stock = juego.Stock - 1;
                if (juego.Stock == 0)
                {
                    juego.Activo = false;
                }
                Pedido pedido = new Pedido
                {
                    UsuarioId = idUsuario,
                    Fecha = DateTime.Now,
                    Total = precio
                };
                this.context.Pedidos.Add(pedido);
                await this.context.SaveChangesAsync();
                PedidoDetalle detalle = new PedidoDetalle
                {
                    PedidoId = pedido.Id,
                    JuegoId = idJuego,
                    Cant = 1,
                    Precio = precio
                };
                this.context.PedidoDetalle.Add(detalle);
                await this.context.SaveChangesAsync();
            }
          
        }

        public async Task<List<CompraRealizada>> GetJuegosCompradosAsync(int idUsuario)
        {
            return await this.context.ComprasRealizadas
                             .Where(x => x.UsuarioId == idUsuario)
                             .OrderByDescending(x => x.FechaCompra)
                             .ToListAsync();
        }
        public async Task ProcesarPedidoCarritoAsync(int idUsuario, List<Juego> carrito)
        {
            var carritoAgrupado = carrito
                .GroupBy(j => j.Id)
                .Select(grupo => new {
                    IdJuego = grupo.Key,
                    CantidadTotal = grupo.Count(),
                    PrecioUnitario = grupo.First().Precio
                }).ToList();

            
            decimal totalPedido = carritoAgrupado.Sum(item => item.PrecioUnitario * item.CantidadTotal);


            Pedido pedido = new Pedido
            {
                UsuarioId = idUsuario,
                Fecha = DateTime.Now,
                Total = totalPedido
            };
            this.context.Pedidos.Add(pedido);
  
            await this.context.SaveChangesAsync();

            foreach (var item in carritoAgrupado)
            {
                Juego dbJuego = await this.context.Juegos.FindAsync(item.IdJuego);

                if (dbJuego != null && dbJuego.Stock >= item.CantidadTotal)
                {
                    // Restamos la cantidad agrupada del stock real
                    dbJuego.Stock -= item.CantidadTotal;

                    if (dbJuego.Stock == 0)
                    {
                        dbJuego.Activo = false;
                    }
                    PedidoDetalle detalle = new PedidoDetalle
                    {
                        PedidoId = pedido.Id,
                        JuegoId = dbJuego.Id,
                        Cant = item.CantidadTotal, 
                        Precio = dbJuego.Precio
                    };
                    this.context.PedidoDetalle.Add(detalle);
                }
            }
            await this.context.SaveChangesAsync();
        }
        public async Task<List<Resena>> GetResenasJuegoAsync(int idJuego)
        {
            return await this.context.Resenas
                .Where(z => z.JuegoId == idJuego).OrderByDescending(z => z.Fecha).ToListAsync();
        }
        public async Task InsertarResenaAsync(int idUsuario, int idJuego, int nota, string comentario)
        {
            Resena resena = new Resena
            {
                UsuarioId = idUsuario,
                JuegoId = idJuego,
                Nota = nota,
                Comentario = comentario,
                Fecha = DateTime.Now
            };

            this.context.Resenas.Add(resena);
            await this.context.SaveChangesAsync();
        }

        public async Task<List<Juego>> GetJuegosMasVendidosAsync()
        {
            return await this.context.Juegos
                .Include(j => j.PedidoDetalles)
                .OrderByDescending(j => j.PedidoDetalles.Count) 
                .Take(5) 
                .ToListAsync();
        }
        public async Task<List<Resena>> GetAllResenasAsync()
        {
            return await this.context.Resenas
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }
      

    }
}
