
using System.Web.Mvc;

namespace Integrador.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session == null || session["UsuarioId"] == null || session["Rol"] == null || session["Rol"].ToString() != "Administrador")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "area", "" }
                    });
            }
        }
    }
}