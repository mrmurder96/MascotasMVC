
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;

namespace Integrador.Filters
{
    /// <summary>
    /// Filtro que valida si el usuario tiene acceso al área Admin basándose en permisos de base de datos.
    /// Ahora permite acceso granular - cualquier usuario con permisos de admin puede acceder.
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            
            // Validar que el usuario esté autenticado
            if (session == null || session["UsuarioId"] == null)
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

            // Obtener el nombre del controlador actual
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            
            // Validar si el usuario tiene permisos para este controlador en el área Admin
            if (!TienePermisoAdmin(session, controllerName))
            {
                // Si no tiene permisos, redirigir al login o página de acceso denegado
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "area", "" }
                    });
            }
        }

        private bool TienePermisoAdmin(System.Web.HttpSessionStateBase session, string controllerName)
        {
            var rol = session["Rol"]?.ToString();
            if (string.IsNullOrEmpty(rol))
                return false;

            // Verificar si los permisos están en sesión (optimización)
            var permisosSession = session["PermisosUsuario"] as System.Collections.Generic.List<Permisos>;
            
            if (permisosSession != null)
            {
                // Usar permisos de sesión
                return permisosSession.Any(p => p.ControllerName == controllerName && p.EstaActivo);
            }

            // Si no están en sesión, consultar la BD
            using (var db = new adopEntities())
            {
                var permisosIds = db.RolPermisos
                    .Where(rp => rp.Rol == rol && rp.TieneAcceso)
                    .Select(rp => rp.PermisoId)
                    .ToList();

                var tienePermiso = db.Permisos
                    .Any(p => p.EstaActivo && 
                              permisosIds.Contains(p.Id) && 
                              p.ControllerName == controllerName);

                return tienePermiso;
            }
        }
    }
}