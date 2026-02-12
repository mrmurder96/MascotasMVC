

using System.Web.Mvc;

namespace Integrador.Filters
{
    public class ClienteAuthorizeAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session == null || session["UsuarioId"] == null || session["Rol"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "area", "" }
                    });
                return;
            }

            var userRole = session["Rol"].ToString();
            // Permitir acceso solo a usuarios con rol "Ciudadano"
            // Los administradores deben usar su propia área
            if (userRole != "Ciudadano")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "AccesoDenegado" },
                        { "area", "" }
                    });
            }
        }
    }
}

