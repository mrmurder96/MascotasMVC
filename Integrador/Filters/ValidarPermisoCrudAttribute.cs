using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Integrador.Filters
{
    /// <summary>
    /// Atributo para validar permisos CRUD a nivel de acción
    /// Uso: [ValidarPermisoCrud(Operacion = "Crear")]
    /// </summary>
    public class ValidarPermisoCrudAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Operación CRUD a validar: "Crear", "Leer", "Actualizar", "Eliminar"
        /// </summary>
        public string Operacion { get; set; }

        /// <summary>
        /// Nombre del controlador (opcional, se detecta automáticamente)
        /// </summary>
        public string ControllerName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            
            // Verificar que hay sesión activa
            if (session == null || session["UsuarioId"] == null || session["Rol"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "area", "" }
                    });
                return;
            }

            var rol = session["Rol"].ToString();

            // Administradores tienen acceso completo a todo
            if (rol == "Administrador")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            // Obtener el controlador
            string controllerName = ControllerName ?? filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            // Verificar permiso CRUD
            if (!TienePermisoCrud(rol, controllerName, Operacion, session))
            {
                // Redirigir a página de acceso denegado o mostrar error
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccesoDenegado.cshtml",
                    ViewData = new ViewDataDictionary
                    {
                        { "Mensaje", $"No tienes permiso para {Operacion} en {controllerName}" }
                    }
                };
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private bool TienePermisoCrud(string rol, string controllerName, string operacion, System.Web.HttpSessionStateBase session)
        {
            // Construir la clave de sesión para el permiso CRUD
            var key = $"PermisoCrud_{rol}_{controllerName}_{operacion}";
            
            // Verificar en sesión (en producción esto vendría de BD)
            var permiso = session[key];
            
            if (permiso != null && permiso is bool)
            {
                return (bool)permiso;
            }

            // Por defecto, denegar acceso para ciudadanos
            // Solo permitir "Leer" por defecto
            if (operacion == "Leer")
            {
                return true;
            }

            return false;
        }
    }
}
