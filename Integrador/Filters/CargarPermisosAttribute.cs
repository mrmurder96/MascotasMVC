using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;

namespace Integrador.Filters
{
    public class CargarPermisosAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session != null && session["Rol"] != null)
            {
                var rol = session["Rol"].ToString();
                var permisos = ObtenerPermisosPorRol(rol);
                filterContext.Controller.ViewBag.Permisos = permisos;
            }
            
            base.OnActionExecuting(filterContext);
        }

        private List<Permisos> ObtenerPermisosPorRol(string rol)
        {
            var todosPermisos = new List<Permisos>
            {
                new Permisos { Id = 1, Nombre = "Dashboard", ControllerName = "Admin", ActionName = "Index", Icono = "??", Orden = 1, EsActivo = true },
                new Permisos { Id = 2, Nombre = "Usuarios", ControllerName = "Usuarios", ActionName = "Index", Icono = "??", Orden = 2, EsActivo = true },
                new Permisos { Id = 3, Nombre = "Mascotas", ControllerName = "Mascotas", ActionName = "Index", Icono = "??", Orden = 3, EsActivo = true },
                new Permisos { Id = 4, Nombre = "Campañas", ControllerName = "Campanas", ActionName = "Index", Icono = "??", Orden = 4, EsActivo = true },
                new Permisos { Id = 5, Nombre = "Adopciones", ControllerName = "Adopciones", ActionName = "Index", Icono = "??", Orden = 5, EsActivo = true },
                new Permisos { Id = 6, Nombre = "Centros", ControllerName = "Ciudadano", ActionName = "Centros", Icono = "??", Orden = 6, EsActivo = true },
                new Permisos { Id = 7, Nombre = "Perfil", ControllerName = "Ciudadano", ActionName = "Perfil", Icono = "??", Orden = 7, EsActivo = true },
                new Permisos { Id = 8, Nombre = "Notificaciones", ControllerName = "Ciudadano", ActionName = "Notificaciones", Icono = "??", Orden = 8, EsActivo = true },
                new Permisos { Id = 9, Nombre = "Permisos", ControllerName = "Permisos", ActionName = "Index", Icono = "??", Orden = 9, EsActivo = true }
            };

            if (rol == "Administrador")
            {
                return todosPermisos;
            }
            else if (rol == "Ciudadano")
            {
                // Por defecto, ciudadanos solo ven estos
                var permisosCiudadano = new List<int> { 6, 7, 8 };
                return todosPermisos.Where(p => permisosCiudadano.Contains(p.Id)).ToList();
            }

            return new List<Permisos>();
        }
    }
}
