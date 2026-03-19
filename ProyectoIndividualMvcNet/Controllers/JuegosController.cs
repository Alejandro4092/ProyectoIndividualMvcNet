using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIndividualMvcNet.Extensions;
using ProyectoIndividualMvcNet.Filters;
using ProyectoIndividualMvcNet.Helpers;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;

namespace ProyectoIndividualMvcNet.Controllers
{
    public class JuegosController : Controller
    {
        private IJuegoRepository repo;
        private ImageHelper imageHelper;

        public JuegosController(IJuegoRepository repo, ImageHelper imageHelper)
        {
            this.repo = repo;
            this.imageHelper = imageHelper;
        }

        public async Task<IActionResult> Index(
            string? texto,
            string? genero,
            string? plataforma,
            int? posicion
        )
        {
            if (posicion == null || posicion < 1)
            {
                posicion = 1;
            }

            const int pageSize = 10;

            int totalJuegos = await this.repo.GetNumeroJuegosFiltradosSpAsync(
                texto,
                genero,
                plataforma
            );

            int numeroPaginas = (int)Math.Ceiling(totalJuegos / (double)pageSize);
            if (numeroPaginas < 1)
            {
                numeroPaginas = 1;
            }

            if (posicion.Value > numeroPaginas)
            {
                posicion = numeroPaginas;
            }

            ViewData["TEXTO"] = texto;
            ViewData["GENERO"] = genero;
            ViewData["PLATAFORMA"] = plataforma;

            ViewData["NUMEROREGISTROS"] = numeroPaginas;
            ViewData["POSICIONACTUAL"] = posicion.Value;
            ViewData["TOTALJUEGOS"] = totalJuegos;

            var juegos = await this.repo.GetGrupoJuegosFiltradosSpAsync(
                posicion.Value,
                pageSize,
                texto,
                genero,
                plataforma
            );

            return View(juegos);
        }

        public async Task<IActionResult> Details(int idjuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idjuego);

            if (juego == null)
            {
                return NotFound();
            }

            ViewBag.Resenas = await this.repo.GetResenasJuegoAsync(idjuego);
            return View(juego);
        }

        [AuthorizeUsuarios]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostResena(int idJuego, int nota, string comentario)
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            if (nota < 1 || nota > 5 || string.IsNullOrWhiteSpace(comentario))
            {
                TempData["ERROR"] = "Datos de reseña no válidos.";
                return RedirectToAction("Details", new { idjuego = idJuego });
            }

            await this.repo.InsertarResenaAsync(user.IdUsuario, idJuego, nota, comentario.Trim());
            TempData["MENSAJE"] = "¡Reseña publicada con éxito!";
            return RedirectToAction("Details", new { idjuego = idJuego });
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Create(Juego juego, IFormFile imagenFile)
        {
            try
            {
                string imagenBase64 =
                    "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var imageResult = await this.imageHelper.SaveImageAndGetBase64Async(imagenFile);
                    if (imageResult != null)
                    {
                        imagenBase64 = imageResult.Base64;
                    }
                }

                await this.repo.InsertJuegoAsync(
                    juego.Titulo,
                    juego.Descripcion,
                    juego.Precio,
                    juego.Stock,
                    juego.Genero,
                    juego.Plataforma,
                    imagenBase64,
                    juego.Activo
                );

                TempData["MENSAJE"] = "Juego creado exitosamente (imagen guardada en BD y disco)";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = "Error al crear el juego: " + ex.Message;
                return View(juego);
            }
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int idjuego)
        {
            Juego juego = await this.repo.FindJuegoAsync(idjuego);

            if (juego == null)
            {
                return NotFound();
            }
            return View(juego);
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> Edit(Juego juego, IFormFile imagenFile)
        {
            try
            {
                var juegoActual = await this.repo.FindJuegoAsync(juego.Id);

                if (juegoActual == null)
                {
                    return NotFound();
                }

                string imagenBase64 = juegoActual.Img;

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    var imageResult = await this.imageHelper.SaveImageAndGetBase64Async(imagenFile);
                    if (imageResult != null)
                    {
                        imagenBase64 = imageResult.Base64;
                    }
                }

                await this.repo.EditJuegoAsync(
                    juego.Id,
                    juego.Titulo,
                    juego.Descripcion,
                    juego.Precio,
                    juego.Stock,
                    juego.Genero,
                    juego.Plataforma,
                    imagenBase64,
                    juego.Activo
                );

                TempData["MENSAJE"] = "Juego actualizado exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = "Error al actualizar el juego: " + ex.Message;
                return View(juego);
            }
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int idjuego)
        {
            try
            {
                var juego = await this.repo.FindJuegoAsync(idjuego);
                if (juego != null)
                {
                    this.imageHelper.DeleteImageFile(juego.Img);
                }

                await this.repo.EliminarJuegoAsync(idjuego);
                TempData["MENSAJE"] = "Juego eliminado exitosamente";
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = "Error al eliminar el juego: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> Comprar(int idJuego, decimal precio)
        {
            try
            {
                Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
                if (user == null)
                {
                    TempData["ERROR"] = "Tu sesión ha caducado. Inicia sesión de nuevo.";
                    return RedirectToAction("Login", "Usuarios");
                }

                await this.repo.ComprarJuegoAsync(user.IdUsuario, idJuego, precio);
                TempData["MENSAJE"] = "¡Compra realizada con éxito!";
                return RedirectToAction("MisCompras");
            }
            catch (Exception ex)
            {
                TempData["ERROR"] = $"Error al comprar: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> MisCompras()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            var misJuegos = await this.repo.GetJuegosCompradosAsync(user.IdUsuario);
            return View(misJuegos);
        }

        [AuthorizeUsuarios]
        [HttpPost]
        public IActionResult AgregarAlCarrito(int idJuego, int cantidad = 1)
        {
            if (cantidad < 1)
                cantidad = 1;

            List<Juego> carrito =
                HttpContext.Session.GetObject<List<Juego>>("CARRITO") ?? new List<Juego>();

            Juego juego = this.repo.FindJuegoAsync(idJuego).Result;

            if (juego != null)
            {
                int cantidadEnCarrito = carrito.Count(j => j.Id == idJuego);
                if (cantidadEnCarrito + cantidad > juego.Stock)
                {
                    TempData["ERROR"] =
                        $"No hay suficiente stock de '{juego.Titulo}'. Disponible: {juego.Stock}, ya tienes en carrito: {cantidadEnCarrito}";
                    return RedirectToAction("Index");
                }

                for (int i = 0; i < cantidad; i++)
                {
                    carrito.Add(juego);
                }

                HttpContext.Session.SetObject("CARRITO", carrito);
                TempData["MENSAJE"] =
                    $"✓ Se añadieron {cantidad} unidad(es) de '{juego.Titulo}' al carrito";
            }
            else
            {
                TempData["ERROR"] = "Juego no encontrado";
            }

            return RedirectToAction("Index");
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PanelEstadisticas()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");

            if (user == null || user.RolId != 1)
            {
                return RedirectToAction("Index");
            }

            ViewBag.TopVentas = await this.repo.GetJuegosMasVendidosAsync();

            ViewBag.DistribucionGeneros = await this.repo.GetDistribucionJuegosPorGeneroAsync();

            ViewBag.TendenciaVentas = await this.repo.GetTendenciaVentasMensualesAsync(6);

            return View();
        }

        [AuthorizeUsuarios]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AdminJuegos()
        {
            var juegos = await this.repo.GetTodosJuegosAsync();
            return View(juegos);
        }
    }
}
