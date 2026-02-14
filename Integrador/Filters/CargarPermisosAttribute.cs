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
                // Permisos de Administrador
                new Permisos { Id = 1, Nombre = "Dashboard", ControllerName = "Admin", ActionName = "Index", Icono = "fa-chart-line", Orden = 1, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 2, Nombre = "Permisos", ControllerName = "Permisos", ActionName = "Index", Icono = "fa-lock", Orden = 2, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 3, Nombre = "Mascotas Admin", ControllerName = "Mascotas", ActionName = "Index", Icono = "fa-paw", Orden = 3, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 4, Nombre = "Solicitudes", ControllerName = "Solicitudes", ActionName = "Index", Icono = "fa-file-alt", Orden = 4, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 5, Nombre = "Seguimientos", ControllerName = "Seguimientos", ActionName = "Index", Icono = "fa-clipboard-list", Orden = 5, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 6, Nombre = "Mascotas Perdidas", ControllerName = "MascotasPerdidas", ActionName = "Index", Icono = "fa-search", Orden = 6, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 7, Nombre = "Categorías", ControllerName = "Categorias", ActionName = "Index", Icono = "fa-folder", Orden = 7, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 8, Nombre = "Refugios", ControllerName = "Refugios", ActionName = "Index", Icono = "fa-home", Orden = 8, EstaActivo = true, Area = "Admin" },
                new Permisos { Id = 9, Nombre = "Reportes", ControllerName = "Reportes", ActionName = "Index", Icono = "fa-chart-bar", Orden = 9, EstaActivo = true, Area = "Admin" },
                
                // Permisos de Ciudadano
                new Permisos { Id = 10, Nombre = "Inicio", ControllerName = "Ciudadano", ActionName = "Index", Icono = "fa-home", Orden = 10, EstaActivo = true, Area = null },
                new Permisos { Id = 11, Nombre = "Mascotas", ControllerName = "Mascotas", ActionName = "Index", Icono = "fa-paw", Orden = 11, EstaActivo = true, Area = null },
                new Permisos { Id = 12, Nombre = "Mis Adopciones", ControllerName = "Ciudadano", ActionName = "MisAdopciones", Icono = "fa-heart", Orden = 12, EstaActivo = true, Area = null },
                new Permisos { Id = 13, Nombre = "Perfil", ControllerName = "Ciudadano", ActionName = "Perfil", Icono = "fa-user", Orden = 13, EstaActivo = true, Area = null },
                new Permisos { Id = 14, Nombre = "Notificaciones", ControllerName = "Ciudadano", ActionName = "Notificaciones", Icono = "fa-bell", Orden = 14, EstaActivo = true, Area = null }
            };

            if (rol == "Administrador")
            {
                // Admin: permisos 1-9
                var permisosAdmin = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                return todosPermisos.Where(p => permisosAdmin.Contains(p.Id)).OrderBy(p => p.Orden).ToList();
            }
            else if (rol == "Ciudadano")
            {
                // Ciudadano: permisos 10-14 (SIN Centros)
                var permisosCiudadano = new List<int> { 10, 11, 12, 13, 14 };
                return todosPermisos.Where(p => permisosCiudadano.Contains(p.Id)).OrderBy(p => p.Orden).ToList();
            }

            return new List<Permisos>();
        }
    }
}

