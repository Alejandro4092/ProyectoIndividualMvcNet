using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoIndividualMvcNet.Repositories
{
    public class UsuarioRankingDto
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Imagen { get; set; }
        public int TotalCompras { get; set; }
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private TiendaJuegosContext context;

        public UsuarioRepository(TiendaJuegosContext context)
        {
            this.context = context;
        }

        // Método para sacar el ID manualmente (como te han pedido)
        private async Task<int> GetMaxIdUsuarioAsync()
        {
            if (await this.context.Usuarios.CountAsync() == 0)
            {
                return 1;
            }
            return await this.context.Usuarios.MaxAsync(z => z.IdUsuario) + 1;
        }

        public async Task RegisterUserAsync(
            string nombre,
            string email,
            string imagen,
            string password
        )
        {
            Usuario user = new Usuario
            {
                Nombre = nombre,
                Email = email,
                Imagen = imagen,
                RolId = 0,
            };
            this.context.Usuarios.Add(user);
            await this.context.SaveChangesAsync();

            UsuarioSecurity security = new UsuarioSecurity();
            security.IdUsuario = user.IdUsuario;
            security.Salt = HelperTools.GenerateSalt();
            security.Pass = HelperCryptography.EncryptPassword(password, security.Salt);
            security.Password = password;

            this.context.UsuariosSecurity.Add(security);
            await this.context.SaveChangesAsync();
        }

        public async Task<Usuario> LogInUserAsync(string email, string password)
        {
            var vUser = await this.context.VistaUsuariosLogin.FirstOrDefaultAsync(x =>
                x.Email == email
            );
            if (vUser == null)
                return null;

            byte[] temp = HelperCryptography.EncryptPassword(password, vUser.Salt);

            if (HelperTools.CompareArrays(temp, vUser.Pass))
            {
                return new Usuario
                {
                    IdUsuario = vUser.IdUsuario,
                    Nombre = vUser.Nombre,
                    Email = vUser.Email,
                    Imagen = vUser.Imagen,
                    RolId = vUser.RolId,
                };
            }
            return null;
        }

        // Busca un usuario por su ID para cargar el perfil
        public async Task<Usuario> FindUsuarioAsync(int idUsuario)
        {
            return await this.context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        // Actualiza los datos del usuario en la base de datos
        public async Task UpdatePerfilSinPasswordAsync(
            int idUsuario,
            string nombre,
            string email,
            string imagen
        )
        {
            // Buscamos al usuario real en la base de datos
            Usuario user = await this.context.Usuarios.FirstOrDefaultAsync(u =>
                u.IdUsuario == idUsuario
            );

            if (user != null)
            {
                // Actualizamos solo los campos permitidos
                user.Nombre = nombre;
                user.Email = email;
                user.Imagen = imagen;

                // La contraseña (user.Password) ni se toca, se queda la que estaba
                await this.context.SaveChangesAsync();
            }
        }

        // Obtener todos los usuarios de la base de datos
        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await this.context.Usuarios.ToListAsync();
        }

        public async Task DeleteUsuarioAsync(int idUsuario)
        {
            // 1. Borrar Pedidos y sus Detalles (Tablas físicas)
            var pedidos = await this
                .context.Pedidos.Where(p => p.UsuarioId == idUsuario)
                .ToListAsync();

            if (pedidos.Any())
            {
                var pedidoIds = pedidos.Select(p => p.Id).ToList();
                var detalles = await this
                    .context.PedidoDetalle.Where(d => pedidoIds.Contains(d.PedidoId))
                    .ToListAsync();

                if (detalles.Any())
                {
                    this.context.PedidoDetalle.RemoveRange(detalles);
                }
                this.context.Pedidos.RemoveRange(pedidos);
            }

            // 2. Borrar reseñas (Tabla física)
            var resenas = await this
                .context.Resenas.Where(r => r.UsuarioId == idUsuario)
                .ToListAsync();
            if (resenas.Any())
            {
                this.context.Resenas.RemoveRange(resenas);
            }

            // NOTA: Se elimina el bloque de ComprasRealizadas
            // porque es una VISTA no actualizable.
            // Al borrar Pedidos y Detalles arriba, la vista se actualizará sola.

            // 3. Borrar seguridad
            var userSecurity = await this.context.UsuariosSecurity.FirstOrDefaultAsync(s =>
                s.IdUsuario == idUsuario
            );
            if (userSecurity != null)
            {
                this.context.UsuariosSecurity.Remove(userSecurity);
            }

            // 4. Borrar usuario
            var user = await this.context.Usuarios.FirstOrDefaultAsync(u =>
                u.IdUsuario == idUsuario
            );
            if (user != null)
            {
                this.context.Usuarios.Remove(user);
            }

            await this.context.SaveChangesAsync();
        }

        public async Task<List<Resena>> GetTodasResenasAsync()
        {
            return await this.context.Resenas.OrderByDescending(r => r.Fecha).ToListAsync();
        }

        public async Task DeleteResenaAsync(int idResena)
        {
            Resena resena = await this.context.Resenas.FindAsync(idResena);
            if (resena != null)
            {
                this.context.Resenas.Remove(resena);
                await this.context.SaveChangesAsync();
            }
        }

        // Ranking de usuarios con más compras
        public async Task<List<UsuarioRankingDto>> GetRankingUsuariosPorComprasAsync(int top = 10)
        {
            var ranking = await this.context.Usuarios
                .Select(u => new UsuarioRankingDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Imagen = u.Imagen,
                    TotalCompras = u.Pedidos.Count()
                })
                .OrderByDescending(u => u.TotalCompras)
                .ThenBy(u => u.Nombre)
                .Take(top)
                .ToListAsync();
            return ranking;
        }
    }
}
