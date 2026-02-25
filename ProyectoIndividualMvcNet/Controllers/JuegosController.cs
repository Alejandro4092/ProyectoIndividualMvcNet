using Microsoft.AspNetCore.Mvc;
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
            ViewBag.IsAdmin = true;
            return View(juegos);
        }
        public async Task<IActionResult> Details(int idjuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idjuego);
            return View(juego);
        }
        public async Task<IActionResult> Delete(int idjuego)
        {
            //await this.repo.DelteEnfermoAsync(inscripcion);
            await this.repo.EliminarJuegoAsync(idjuego);
            return RedirectToAction("Index");
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
        // 1. GET: Mostrar el formulario con los datos actuales
        public async Task<IActionResult> Edit(int idjuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idjuego);
            if (juego == null)
            {
                return NotFound();
            }
            return View(juego);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Juego juego)
        {
            await this.repo.EditarJuegoAsync(
                juego.Id,
                juego.Titulo,
                juego.Descripcion,juego.Precio,juego.Stock,juego.Genero,juego.Plataforma,juego.Img,juego.Activo
            );

            return RedirectToAction("Index");
        }
    }
}
