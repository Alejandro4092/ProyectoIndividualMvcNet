using Microsoft.AspNetCore.Mvc;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;
using ProyectoIndividualMvcNet.Extensions;
using Microsoft.AspNetCore.Http;

namespace ProyectoIndividualMvcNet.Controllers
{
    public class UsuariosController : Controller
    {
        private UsuarioRepository repo;

        public UsuariosController(UsuarioRepository repo)
        {
            this.repo = repo;
        }


        public async Task<IActionResult> Perfil()
        {
            Usuario sessionUser = HttpContext.Session.GetObject<Usuario>("USUARIO");

            if (sessionUser == null)
            {
                return RedirectToAction("Login");
            }

            Usuario user = await this.repo.FindUsuarioAsync(sessionUser.IdUsuario);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Perfil(Usuario user)
        {
            await this.repo.UpdatePerfilSinPasswordAsync(
                user.IdUsuario,
                user.Nombre,
                user.Email,
                user.Imagen
            );
            Usuario usuarioActualizado = await this.repo.FindUsuarioAsync(user.IdUsuario);
            HttpContext.Session.SetObject("USUARIO", usuarioActualizado);
            HttpContext.Session.SetString("NOMBRE", usuarioActualizado.Nombre);
            HttpContext.Session.SetString("IMAGEN", usuarioActualizado.Imagen);

            ViewData["MENSAJE"] = "¡Perfil actualizado correctamente!";

            return View(usuarioActualizado);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register
            (string nombre, string email, string imagen, string password)
        {
            await this.repo.RegisterUserAsync(nombre, email, imagen, password);
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetObject<Usuario>("USUARIO") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario user = await this.repo.LogInUserAsync(email, password);

            if (user != null)
            {
                HttpContext.Session.SetObject("USUARIO", user);
                HttpContext.Session.SetString("NOMBRE", user.Nombre);
                HttpContext.Session.SetString("IMAGEN", user.Imagen);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["MENSAJE"] = "Usuario o contraseña incorrectos";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Usuarios");
        }
        // GET: /Usuarios/AdminUsuarios
        public async Task<IActionResult> AdminUsuarios()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null || user.RolId != 1)
            {
                TempData["ERROR"] = "Acceso denegado. Se requieren permisos de administrador.";
                return RedirectToAction("Index", "Juegos");
            }
            List<Usuario> usuarios = await this.repo.GetUsuariosAsync();
            return View(usuarios);
        }
        public async Task<IActionResult> DeleteUsuario(int idUsuario)
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null || user.RolId != 1)
            {
                return RedirectToAction("Login");
            }

            await this.repo.DeleteUsuarioAsync(idUsuario);
            TempData["MENSAJE"] = "Usuario eliminado con éxito.";
            return RedirectToAction("AdminUsuarios");
        }
    }
}