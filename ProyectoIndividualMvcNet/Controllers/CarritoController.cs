using Microsoft.AspNetCore.Mvc;
using ProyectoIndividualMvcNet.Extensions;
using ProyectoIndividualMvcNet.Filters;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;

public class CarritoController : Controller
{
    private JuegoRepository repo;

    public CarritoController(JuegoRepository repo)
    {
        this.repo = repo;
    }

    public IActionResult Index()
    {
        List<Juego> carrito =
            HttpContext.Session.GetObject<List<Juego>>("CARRITO") ?? new List<Juego>();
        return View(carrito);
    }

    [AuthorizeUsuarios]
    public async Task<IActionResult> FinalizarCompra()
    {
        try
        {
            Usuario user = HttpContext.Session.GetObject<Usuario>("USUARIO");
            List<Juego> carrito = HttpContext.Session.GetObject<List<Juego>>("CARRITO");

            if (user == null)
            {
                TempData["ERROR"] = "Tu sesión ha caducado. Inicia sesión de nuevo.";
                return RedirectToAction("Login", "Usuarios");
            }

            if (carrito == null || carrito.Count == 0)
            {
                TempData["ERROR"] = "El carrito está vacío.";
                return RedirectToAction("Index", "Juegos");
            }

            await this.repo.ProcesarPedidoCarritoAsync(user.IdUsuario, carrito);
            HttpContext.Session.Remove("CARRITO");

            TempData["MENSAJE"] = "¡Pedido procesado con éxito! Gracias por tu compra.";
            return RedirectToAction("MisCompras", "Juegos");
        }
        catch (Exception ex)
        {
            TempData["ERROR"] = $"Error al finalizar compra: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    public IActionResult Quitar(int idjuego)
    {
        List<Juego> carrito =
            HttpContext.Session.GetObject<List<Juego>>("CARRITO") ?? new List<Juego>();
        var juegoAEliminar = carrito.FirstOrDefault(j => j.Id == idjuego);
        if (juegoAEliminar != null)
        {
            carrito.Remove(juegoAEliminar);
            HttpContext.Session.SetObject("CARRITO", carrito);
            TempData["MENSAJE"] = $"{juegoAEliminar.Titulo} eliminado del carrito.";
        }

        return RedirectToAction("Index");
    }
}
