using Microsoft.AspNetCore.Authentication;
using ProyectoIndividualMvcNet.Extensions;
using ProyectoIndividualMvcNet.Models;
using ProyectoIndividualMvcNet.Repositories;
using System.Security.Claims;

namespace ProyectoIndividualMvcNet.Middlewares
{
    public class SessionRecoveryMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionRecoveryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUsuarioRepository repo)
        {
            // Solo actuar si hay cookie de claims válida pero la sesión está vacía
            if (
                context.User?.Identity?.IsAuthenticated == true
                && context.Session.GetObject<Usuario>("USUARIO") == null
            )
            {
                string idStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(idStr, out int userId))
                {
                    Usuario user = await repo.FindUsuarioAsync(userId);

                    if (user != null)
                    {
                        // Reconstruir sesión completa
                        context.Session.SetObject("USUARIO", user);
                        context.Session.SetString("NOMBRE", user.Nombre);
                        context.Session.SetString("IMAGEN", user.Imagen ?? "");
                    }
                    else
                    {
                        // El usuario ya no existe en BD → cerrar sesión y mandar al login
                        await context.SignOutAsync();
                        context.Session.Clear();
                        context.Response.Redirect("/Usuarios/Login");
                        return;
                    }
                }
                else
                {
                    // Claims corruptos → cerrar sesión y mandar al login
                    await context.SignOutAsync();
                    context.Session.Clear();
                    context.Response.Redirect("/Usuarios/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}