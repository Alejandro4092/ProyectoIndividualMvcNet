using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndividualMvcNet.Extensions;
using ProyectoIndividualMvcNet.Filters;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;

namespace ProyectoIndividualMvcNet.Controllers
{
    public class UsuariosController : Controller
    {
        private IUsuarioRepository repo;
        private ImageHelper imageHelper;

        public UsuariosController(IUsuarioRepository repo, ImageHelper imageHelper)
        {
            this.repo = repo;
            this.imageHelper = imageHelper;
        }

       
        private async Task<Usuario> GetUsuarioFromSessionOrClaimsAsync()
        {
            // 1. Intentar desde sesión primero (caso normal)
            Usuario sessionUser = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (sessionUser != null) return sessionUser;

            // 2. Sesión vacía pero cookie de claims aún válida → reconstruir
            if (User?.Identity?.IsAuthenticated == true)
            {
                string idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int userId))
                {
                    Usuario user = await this.repo.FindUsuarioAsync(userId);
                    if (user != null)
                    {
                        HttpContext.Session.SetObject("USUARIO", user);
                        HttpContext.Session.SetString("NOMBRE", user.Nombre);
                        HttpContext.Session.SetString("IMAGEN", user.Imagen ?? "");
                    }
                    return user;
                }
            }

            return null;
        }


        public IActionResult ErrorAcceso()
        {
            return View();
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Perfil()
        {
            Usuario sessionUser = await GetUsuarioFromSessionOrClaimsAsync();
            if (sessionUser == null) return RedirectToAction("Login");

            Usuario user = await this.repo.FindUsuarioAsync(sessionUser.IdUsuario);
            return View(user);
        }

        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> Perfil(Usuario user, IFormFile avatarFile)
        {
            string imagenBase64 = user.Imagen;
            if (avatarFile != null && avatarFile.Length > 0)
            {
                imagenBase64 = await this.imageHelper.ConvertToBase64Async(avatarFile);
            }

            await this.repo.UpdatePerfilSinPasswordAsync(
                user.IdUsuario,
                user.Nombre,
                user.Email,
                imagenBase64
            );

            Usuario usuarioActualizado = await this.repo.FindUsuarioAsync(user.IdUsuario);

            HttpContext.Session.SetObject("USUARIO", usuarioActualizado);
            HttpContext.Session.SetString("NOMBRE", usuarioActualizado.Nombre);
            HttpContext.Session.SetString("IMAGEN", usuarioActualizado.Imagen ?? "");

            ViewData["MENSAJE"] = "¡Perfil actualizado correctamente!";
            return View(usuarioActualizado);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            string nombre,
            string email,
            IFormFile imagenFile,
            string password
        )
        {
            string imagenBase64 = null;
            if (imagenFile != null && imagenFile.Length > 0)
            {
                imagenBase64 = await this.imageHelper.ConvertToBase64Async(imagenFile);
            }

            await this.repo.RegisterUserAsync(nombre, email, imagenBase64, password);
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Juegos");

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

            HttpContext.Session.SetObject("USUARIO", user);
            HttpContext.Session.SetString("NOMBRE", user.Nombre);
            HttpContext.Session.SetString("IMAGEN", user.Imagen ?? "");

            ClaimsIdentity identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Email ?? user.Nombre ?? "USER"));

            if (user.RolId == 1)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "ADMIN"));
                identity.AddClaim(new Claim("Admin", "true"));
            }
            else
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "USER"));
            }

            // IsPersistent = true → la cookie sobrevive al cierre del navegador/app
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                }
            );

         
            if (TempData["controller"] != null && TempData["action"] != null)
            {
                string controller = TempData["controller"]!.ToString()!;
                string action = TempData["action"]!.ToString()!;
                string httpMethod = TempData["httpMethod"]?.ToString() ?? "GET";

                if (httpMethod == "POST")
                    return RedirectToAction("Index", "Juegos");

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
            Usuario user = await GetUsuarioFromSessionOrClaimsAsync();
            if (user == null || user.RolId != 1)
                return RedirectToAction("Index", "Juegos");

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

        [AuthorizeUsuarios]
        public async Task<IActionResult> Ranking()
        {
            var ranking = await this.repo.GetRankingUsuariosPorComprasAsync(20);
            return View(ranking);
        }
    }
}