using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ProyectoIndividualMvcNet.Filters
{
    public class AuthorizeUsuariosAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Guardar ruta actual para volver tras login
            string controller = context.RouteData.Values["controller"]?.ToString() ?? "Home";
            string action = context.RouteData.Values["action"]?.ToString() ?? "Index";
            var id = context.RouteData.Values["id"] ?? context.RouteData.Values["idjuego"];

            var provider = context.HttpContext.RequestServices.GetService<ITempDataProvider>();
            if (provider != null)
            {
                var tempData = provider.LoadTempData(context.HttpContext);
                tempData["controller"] = controller;
                tempData["action"] = action;

                if (id != null) tempData["id"] = id.ToString();
                else tempData.Remove("id");

                provider.SaveTempData(context.HttpContext, tempData);
            }

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(
                    new { controller = "Usuarios", action = "Login" }
                ));
            }
        }
    }
}