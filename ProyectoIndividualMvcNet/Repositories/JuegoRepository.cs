using Humanizer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task InsertarJuegoAsync(string titulo, string descripcion, decimal precio, int stock, string genero, string plataforma, string img, bool activo)
        {
            // El SQL con los nombres de los parámetros del Procedure
            string sql = "sp_InsertarJuego @titulo, @descripcion, @precio, @stock, @genero, @plataforma, @img, @activo";

            SqlParameter pamTit = new SqlParameter("@titulo", titulo);
            SqlParameter pamDesc = new SqlParameter("@descripcion",descripcion);
            SqlParameter pamPre = new SqlParameter("@precio", precio);
            SqlParameter pamStock = new SqlParameter("@stock", stock);
            SqlParameter pamGen = new SqlParameter("@genero", genero);
            SqlParameter pamPlat = new SqlParameter("@plataforma",plataforma);
            SqlParameter pamImg = new SqlParameter("@img",img);
            SqlParameter pamAct = new SqlParameter("@activo", activo);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamTit, pamDesc, pamPre, pamStock, pamGen, pamPlat, pamImg, pamAct);
        }
        public async Task EliminarJuegoAsync(int id)
        {
            string sql = "sp_EliminarJuego @id";

            SqlParameter pamId = new SqlParameter("@id", id);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamId);
        }
        public async Task EditarJuegoAsync(int id, string titulo, string descripcion, decimal precio, int stock, string genero, string plataforma, string img, bool activo)
        {
            string sql = "sp_EditarJuego @id, @titulo, @descripcion, @precio, @stock, @genero, @plataforma, @img, @activo";

            SqlParameter pamId = new SqlParameter("@id", id);
            SqlParameter pamTit = new SqlParameter("@titulo", titulo);
            SqlParameter pamDesc = new SqlParameter("@descripcion", descripcion);
            SqlParameter pamPre = new SqlParameter("@precio", precio);
            SqlParameter pamStock = new SqlParameter("@stock", stock);
            SqlParameter pamGen = new SqlParameter("@genero", genero);
            SqlParameter pamPlat = new SqlParameter("@plataforma", plataforma);
            SqlParameter pamImg = new SqlParameter("@img", img);
            SqlParameter pamAct = new SqlParameter("@activo", activo);

            await this.context.Database.ExecuteSqlRawAsync(sql, pamId, pamTit, pamDesc, pamPre, pamStock, pamGen, pamPlat, pamImg, pamAct);
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
                .Take(5) // Top 5
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
