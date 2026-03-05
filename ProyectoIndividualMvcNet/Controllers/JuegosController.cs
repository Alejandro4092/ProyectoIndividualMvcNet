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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Juego juego)
        {
            await this.repo.InsertarJuegoAsync(
                juego.Titulo, juego.Descripcion, juego.Precio,
                juego.Stock, juego.Genero, juego.Plataforma,
                juego.Img, juego.Activo
            );
            return RedirectToAction("Index");
        }

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
                juego.Id, juego.Titulo, juego.Descripcion,
                juego.Precio, juego.Stock, juego.Genero,
                juego.Plataforma, juego.Img, juego.Activo
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

            if (user == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }

            await this.repo.ComprarJuegoAsync(user.IdUsuario, idJuego, precio);
            TempData["MENSAJE"] = "¡Compra realizada con éxito!";
            return RedirectToAction("MisCompras");
        }

        public async Task<IActionResult> MisCompras()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            if (user == null)
            {
                return RedirectToAction("Login", "Usuarios");
            }
            
            var misJuegos = await this.repo.GetJuegosCompradosAsync(user.IdUsuario);
            return View(misJuegos); // Returns List<CompraRealizada>
        }

        [HttpPost]
        public IActionResult AgregarAlCarrito(int idJuego, int cantidad = 1)
        {
            // Validar cantidad
            if (cantidad < 1) cantidad = 1;

            // Obtener carrito de sesión
            List<Juego> carrito = HttpContext.Session.GetObject<List<Juego>>("CARRITO") ?? new List<Juego>();
            
            // Buscar el juego
            Juego juego = this.repo.FindJuegoAsync(idJuego).Result;
            
            if (juego != null)
            {
                // Verificar stock
                int cantidadEnCarrito = carrito.Count(j => j.Id == idJuego);
                if (cantidadEnCarrito + cantidad > juego.Stock)
                {
                    TempData["ERROR"] = $"No hay suficiente stock de '{juego.Titulo}'. Disponible: {juego.Stock}, ya tienes en carrito: {cantidadEnCarrito}";
                    return RedirectToAction("Index"); // Volver al catálogo
                }

                // Añadir las copias al carrito
                for (int i = 0; i < cantidad; i++)
                {
                    carrito.Add(juego);
                }

                HttpContext.Session.SetObject("CARRITO", carrito);
                TempData["MENSAJE"] = $"✓ Se añadieron {cantidad} unidad(es) de '{juego.Titulo}' al carrito";
            }
            else
            {
                TempData["ERROR"] = "Juego no encontrado";
            }

            // CAMBIO CLAVE: Redirigir a Index en lugar de Details
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> PanelEstadisticas()
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");

            if (user == null || user.RolId != 1)
            {
                return RedirectToAction("Index");
            }

            ViewBag.TopVentas = await this.repo.GetJuegosMasVendidosAsync();
            return View();
        }
    }
}