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

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register
            (string nombre, string email, string imagen, string password)
        {
            await this.repo.RegisterUserAsync(nombre, email, imagen, password);
            // Tras el registro, mandamos al usuario al Login
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            // Si el usuario ya está logueado y entra en /Login, lo mandamos a Home
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
            // Limpia toda la sesión
            HttpContext.Session.Clear();
            // Elimina la cookie de sesión del navegador
            HttpContext.Response.Cookies.Delete(".AspNetCore.Session");

            return RedirectToAction("Login", "Usuarios");
        }
    }
}