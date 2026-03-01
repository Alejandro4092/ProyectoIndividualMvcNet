using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using ProyectoIndividualMvcNet.Data;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Models;

namespace ProyectoIndividualMvcNet.Repositories
{
    public class UsuarioRepository
    {
        #region ProcedureVista
//        CREATE VIEW V_USUARIOS_LOGIN AS
//SELECT
//    U.IDUSUARIO, U.NOMBRE, U.EMAIL, U.IMAGEN, U.RolId,
//    S.SALT, S.PASS
//FROM Usuarios U
//JOIN Usuarios_Security S ON U.IDUSUARIO = S.IDUSUARIO
        #endregion
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

        public async Task RegisterUserAsync(string nombre, string email, string imagen, string password)
        {
         
            Usuario user = new Usuario
            {
                Nombre = nombre,
                Email = email,
                Imagen = imagen,
                RolId = 0
            };
            this.context.Usuarios.Add(user);
            await this.context.SaveChangesAsync(); 

       
            UsuarioSecurity security = new UsuarioSecurity();
            security.IdUsuario = user.IdUsuario; 
            security.Salt = HelperTools.GenerateSalt();
            security.Pass = HelperCryptography.EncryptPassword(password, security.Salt);

            this.context.UsuariosSecurity.Add(security);
            await this.context.SaveChangesAsync();
        }

        public async Task<Usuario> LogInUserAsync(string email, string password)
        {
            var vUser = await this.context.VistaUsuariosLogin.FirstOrDefaultAsync(x => x.Email == email);
            if (vUser == null) return null;

            byte[] temp = HelperCryptography.EncryptPassword(password, vUser.Salt);

            if (HelperTools.CompareArrays(temp, vUser.Pass))
            {
                return new Usuario
                {
                    IdUsuario = vUser.IdUsuario,
                    Nombre = vUser.Nombre,
                    Email = vUser.Email,
                    Imagen = vUser.Imagen,
                    RolId = vUser.RolId
                };
            }
            return null;
        }
        // Busca un usuario por su ID para cargar el perfil
        public async Task<Usuario> FindUsuarioAsync(int idUsuario)
        {
            return await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        // Actualiza los datos del usuario en la base de datos
        public async Task UpdatePerfilSinPasswordAsync(int idUsuario, string nombre, string email, string imagen)
        {
            // Buscamos al usuario real en la base de datos
            Usuario user = await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

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

        // Eliminar un usuario por su ID
        public async Task DeleteUsuarioAsync(int idUsuario)
        {
            Usuario user = await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (user != null)
            {

                var userSecurity = await this.context.UsuariosSecurity
                    .FirstOrDefaultAsync(s => s.IdUsuario == idUsuario);
                
                if (userSecurity != null)
                {
                    this.context.UsuariosSecurity.Remove(userSecurity);
                }
                this.context.Usuarios.Remove(user);
                await this.context.SaveChangesAsync();
            }
        }
        public async Task<List<Resena>> GetTodasResenasAsync()
        {
            return await this.context.Resenas
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
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

    }
}