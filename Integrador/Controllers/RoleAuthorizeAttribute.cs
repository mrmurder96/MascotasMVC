
using System.Linq;
using System.Web.Mvc;

namespace Integrador.Filters
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? new string[] { };
        }

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
            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(userRole))
            {
                // Redirigir a página de acceso denegado o al dashboard correspondiente
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
