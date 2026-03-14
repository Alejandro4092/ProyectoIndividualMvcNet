using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndividualMvcNet.Extensions;
using ProyectoIndividualMvcNet.Filters;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;

namespace ProyectoIndividualMvcNet.Controllers
{
    public class UsuariosController : Controller
    {
        private UsuarioRepository repo;

        public UsuariosController(UsuarioRepository repo)
        {
            this.repo = repo;
        }

        public IActionResult ErrorAcceso()
        {
            return View();
        }

        [AuthorizeUsuarios]
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

        [AuthorizeUsuarios]
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
        public async Task<IActionResult> Register(string nombre, string email, string imagen, string password)
        {
            await this.repo.RegisterUserAsync(nombre, email, imagen, password);
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            // Si ya hay cookie auth, fuera del login
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Juegos");
            }

            // Mantengo tu check por sesión también
            if (HttpContext.Session.GetObject<Usuario>("USUARIO") != null)
            {
                return RedirectToAction("Index", "Juegos");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Usuario user = await this.repo.LogInUserAsync(email, password);

            if (user == null)
            {
                ViewData["MENSAJE"] = "Usuario o contraseña incorrectos";
                return View();
            }

            // Mantener sesión (tu app lo usa en varios sitios)
            HttpContext.Session.SetObject("USUARIO", user);
            HttpContext.Session.SetString("NOMBRE", user.Nombre);
            HttpContext.Session.SetString("IMAGEN", user.Imagen);

            // Cookie Auth + Claims
            ClaimsIdentity identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Email ?? user.Nombre ?? "USER"));
            identity.AddClaim(new Claim("Nombre", user.Nombre ?? ""));
            identity.AddClaim(new Claim("Imagen", user.Imagen ?? ""));
            identity.AddClaim(new Claim("RolId", user.RolId.ToString()));

            // Claim Admin para tu policy "AdminOnly"
            if (user.RolId == 1)
            {
                identity.AddClaim(new Claim("Admin", "true"));
                identity.AddClaim(new Claim(ClaimTypes.Role, "ADMIN"));
            }
            else
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "USER"));
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            // Redirección a la ruta guardada por el filtro
            if (TempData["controller"] != null && TempData["action"] != null)
            {
                string controller = TempData["controller"]!.ToString()!;
                string action = TempData["action"]!.ToString()!;

                if (TempData["id"] != null)
                {
                    string id = TempData["id"]!.ToString()!;
                    return RedirectToAction(action, controller, new { id = id });
                }

                return RedirectToAction(action, controller);
            }

            return RedirectToAction("Index", "Juegos");
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AdminUsuarios()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");

            if (user == null || user.RolId != 1)
            {
                return RedirectToAction("Index", "Juegos");
            }

            List<Usuario> usuarios = await this.repo.GetUsuariosAsync();
            ViewBag.Resenas = await this.repo.GetTodasResenasAsync();

            return View(usuarios);
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> DeleteResena(int idResena)
        {
            await this.repo.DeleteResenaAsync(idResena);
            TempData["Mensaje"] = "Reseña eliminada correctamente.";
            return RedirectToAction("AdminUsuarios");
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> DeleteUsuario(int idUsuario)
        {
            await this.repo.DeleteUsuarioAsync(idUsuario);
            TempData["Mensaje"] = "Usuario eliminado correctamente.";
            return RedirectToAction("AdminUsuarios");
        }
    }
}