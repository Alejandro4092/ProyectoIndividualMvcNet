using Microsoft.AspNetCore.Mvc;
using ProyectoIndividualMvcNet.Extensions;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;

namespace ProyectoIndividualMvcNet.Controllers
{
    public class JuegosController : Controller
    {
        private JuegoRepository repo;

        public JuegosController(JuegoRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index(string? texto, string? genero, string? plataforma)
        {
            var juegos = await this.repo.GetJuegosPlataformaGeneroAsync(texto, genero, plataforma);
            ViewData["TEXTO"] = texto;
            ViewData["GENERO"] = genero;
            ViewData["PLATAFORMA"] = plataforma;
            // Nota: Aquí podrías vincular el IsAdmin a la sesión si tienes roles
            ViewBag.IsAdmin = true;
            return View(juegos);
        }

        public async Task<IActionResult> Details(int idjuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idjuego);
            if (juego == null) return NotFound();

            // CARGAMOS LAS RESEÑAS EN EL VIEWBUG USANDO TU MODELO RESENA
            ViewBag.Resenas = await this.repo.GetResenasJuegoAsync(idjuego);

            return View(juego);
        }

        [HttpPost]
        public async Task<IActionResult> PostResena(int idJuego, int nota, string comentario)
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            await this.repo.InsertarResenaAsync(user.IdUsuario, idJuego, nota, comentario);

            TempData["MENSAJE"] = "¡Reseña publicada con éxito!";
            return RedirectToAction("Details", new { idjuego = idJuego });
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Juego juego)
        {
            await this.repo.InsertarJuegoAsync(juego.Titulo, juego.Descripcion, juego.Precio, juego.Stock, juego.Genero, juego.Plataforma, juego.Img, juego.Activo);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int idjuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idjuego);
            if (juego == null) return NotFound();
            return View(juego);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Juego juego)
        {
            await this.repo.EditarJuegoAsync(
                juego.Id,
                juego.Titulo,
                juego.Descripcion, juego.Precio, juego.Stock, juego.Genero, juego.Plataforma, juego.Img, juego.Activo
            );
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int idjuego)
        {
            await this.repo.EliminarJuegoAsync(idjuego);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Comprar(int idJuego, decimal precio)
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null) return RedirectToAction("Login", "Usuarios");

            await this.repo.ComprarJuegoAsync(user.IdUsuario, idJuego, precio);

            TempData["MENSAJE"] = "¡Gracias por tu compra! Ya puedes disfrutar de tu juego.";
            return RedirectToAction("MisCompras");
        }

        public async Task<IActionResult> MisCompras()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null) return RedirectToAction("Login", "Usuarios");

            var misJuegos = await this.repo.GetJuegosCompradosAsync(user.IdUsuario);
            return View(misJuegos);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarAlCarrito(int idJuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idJuego);

            if (juego == null || juego.Stock <= 0)
            {
                TempData["ERROR"] = "El juego no está disponible.";
                return RedirectToAction("Index");
            }

            List<Juego> carrito = HttpContext.Session.GetObject<List<Juego>>("CARRITO") ?? new List<Juego>();

            if (!carrito.Any(j => j.Id == idJuego))
            {
                carrito.Add(juego);
                HttpContext.Session.SetObject("CARRITO", carrito);
                TempData["MENSAJE"] = $"¡{juego.Titulo} añadido al carrito!";
            }
            else
            {
                TempData["INFO"] = "Este juego ya está en tu carrito.";
            }

            return RedirectToAction("Index");
        }
    }
}