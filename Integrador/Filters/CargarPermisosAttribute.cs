using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;

namespace Integrador.Filters
{
    /// <summary>
    /// Filtro que carga los permisos del usuario desde la base de datos y los asigna al ViewBag.
    /// Implementa caché en sesión para optimizar el rendimiento.
    /// Asigna área Admin basándose en los permisos del usuario, no solo en el rol.
    /// </summary>
    public class CargarPermisosAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session != null && session["Rol"] != null)
            {
                var rol = session["Rol"].ToString();
                
                // SIEMPRE recargar permisos (temporal para debug)
                // Comentar esta línea después de resolver el problema
                session["PermisosUsuario"] = null;
                
                // Intentar obtener permisos de la sesión (caché)
                var permisos = session["PermisosUsuario"] as List<Permisos>;
                
                // Si no están en sesión, obtenerlos de la BD y cachearlos
                if (permisos == null)
                {
                    permisos = ObtenerPermisosPorRol(rol);
                    session["PermisosUsuario"] = permisos; // Cachear en sesión
                }
                
                filterContext.Controller.ViewBag.Permisos = permisos;
            }
            
            base.OnActionExecuting(filterContext);
        }

        private List<Permisos> ObtenerPermisosPorRol(string rol)
        {
            using (var db = new adopEntities())
            {
                if (string.IsNullOrEmpty(rol))
                    return new List<Permisos>();

                // Obtener los IDs de permisos asignados a este rol desde la base de datos
                var permisosIds = db.RolPermisos
                    .Where(rp => rp.Rol == rol && rp.TieneAcceso)
                    .Select(rp => rp.PermisoId)
                    .ToList();

                // Obtener los permisos completos
                var permisos = db.Permisos
                    .Where(p => p.EstaActivo && permisosIds.Contains(p.Id))
                    .OrderBy(p => p.Orden)
                    .ToList();

                // Asignar el área a cada permiso según los permisos del usuario
                foreach (var permiso in permisos)
                {
                    permiso.Area = AsignarArea(permiso.ControllerName, permisos);
                }

                return permisos;
            }
        }

        /// <summary>
        /// Asigna el área (Admin o null) basándose en el ID del permiso.
        /// Permisos 1-12 = Admin area
        /// Permisos 13+ = Raíz (ciudadanos)
        /// </summary>
        private string AsignarArea(string controllerName, List<Permisos> permisosUsuario)
        {
            // Controladores que NUNCA van al área Admin (siempre área raíz)
            var controladoresRaizExclusivos = new[] { "Ciudadano", "Account", "Home" };
            if (controladoresRaizExclusivos.Contains(controllerName))
                return null;

            // Buscar el permiso específico del usuario para este controlador
            var permisoUsuario = permisosUsuario.FirstOrDefault(p => p.ControllerName == controllerName);
            
            if (permisoUsuario != null)
            {
                // Permisos 1-12: Área Admin (para administradores)
                // Permisos 13+: Área Raíz (para ciudadanos)
                return permisoUsuario.Id <= 12 ? "Admin" : null;
            }
            
            // Si no tiene permiso para este controlador, área null (raíz)
            return null;
        }
    }
}


