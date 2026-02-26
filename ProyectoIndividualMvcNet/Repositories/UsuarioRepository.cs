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
    }
}