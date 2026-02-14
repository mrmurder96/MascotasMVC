using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class PermisosController : Controller
    {
        private adopEntities db = new adopEntities();

        public ActionResult Index()
        {
            // Obtener todos los permisos desde la base de datos
            var permisosDisponibles = ObtenerPermisosDesdeDB();
            
            // Crear ViewModels para Admin y Ciudadano
            var permisosAdminVM = CrearPermisosViewModel("Administrador", permisosDisponibles);
            var permisosCiudadanoVM = CrearPermisosViewModel("Ciudadano", permisosDisponibles);

            ViewBag.PermisosAdmin = permisosAdminVM;
            ViewBag.PermisosCiudadano = permisosCiudadanoVM;
            ViewBag.Permisos = ViewBag.Permisos ?? ObtenerPermisosPorRol(Session["Rol"]?.ToString());

            return View(permisosDisponibles);
        }

        [HttpPost]
        public JsonResult ActualizarPermiso(string rol, int permisoId, string controllerName, bool tieneAcceso)
        {
            try
            {
                // Buscar o crear el RolPermiso en la base de datos
                var rolPermiso = db.RolPermisos.FirstOrDefault(rp => rp.Rol == rol && rp.PermisoId == permisoId);
                
                if (rolPermiso != null)
                {
                    rolPermiso.TieneAcceso = tieneAcceso;
                    db.Entry(rolPermiso).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    // Crear nuevo permiso
                    rolPermiso = new RolPermisos
                    {
                        Rol = rol,
                        PermisoId = permisoId,
                        TieneAcceso = tieneAcceso,
                        PuedeCrear = false,
                        PuedeLeer = tieneAcceso,
                        PuedeActualizar = false,
                        PuedeEliminar = false,
                        FechaAsignacion = DateTime.Now
                    };
                    db.RolPermisos.Add(rolPermiso);
                }
                
                db.SaveChanges();

                return Json(new { success = true, message = "Permiso actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ActualizarPermisoCrud(string rol, int permisoId, string controllerName, string operacion, bool tienePermiso)
        {
            try
            {
                // Buscar o crear el RolPermiso en la base de datos
                var rolPermiso = db.RolPermisos.FirstOrDefault(rp => rp.Rol == rol && rp.PermisoId == permisoId);
                
                if (rolPermiso == null)
                {
                    // Crear nuevo permiso
                    rolPermiso = new RolPermisos
                    {
                        Rol = rol,
                        PermisoId = permisoId,
                        TieneAcceso = true,
                        PuedeCrear = false,
                        PuedeLeer = true,
                        PuedeActualizar = false,
                        PuedeEliminar = false,
                        FechaAsignacion = DateTime.Now
                    };
                    db.RolPermisos.Add(rolPermiso);
                }

                // Actualizar el permiso específico según la operación
                switch (operacion)
                {
                    case "Crear":
                        rolPermiso.PuedeCrear = tienePermiso;
                        break;
                    case "Leer":
                        rolPermiso.PuedeLeer = tienePermiso;
                        break;
                    case "Actualizar":
                        rolPermiso.PuedeActualizar = tienePermiso;
                        break;
                    case "Eliminar":
                        rolPermiso.PuedeEliminar = tienePermiso;
                        break;
                }

                db.Entry(rolPermiso).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = $"Permiso de {operacion} actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<PermisoConCrudViewModel> CrearPermisosViewModel(string rol, List<Permisos> permisos)
        {
            var result = new List<PermisoConCrudViewModel>();

            // Obtener los permisos del rol para verificar el área
            var permisosRol = ObtenerPermisosPorRol(rol);

            foreach (var permiso in permisos)
            {
                // Asignar el área según el controlador y los permisos del rol
                permiso.Area = AsignarArea(permiso.ControllerName, permisosRol);

                var rolPermiso = db.RolPermisos.FirstOrDefault(rp => rp.Rol == rol && rp.PermisoId == permiso.Id);

                var vm = new PermisoConCrudViewModel
                {
                    Permiso = permiso,
                    TieneAcceso = rolPermiso?.TieneAcceso ?? false,
                    PermisoCrud = new PermisoCrud
                    {
                        PermisoId = permiso.Id,
                        Rol = rol,
                        ControllerName = permiso.ControllerName,
                        PuedeCrear = rolPermiso?.PuedeCrear ?? false,
                        PuedeLeer = rolPermiso?.PuedeLeer ?? false,
                        PuedeActualizar = rolPermiso?.PuedeActualizar ?? false,
                        PuedeEliminar = rolPermiso?.PuedeEliminar ?? false
                    }
                };

                result.Add(vm);
            }

            return result;
        }

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

        private List<Permisos> ObtenerPermisosDesdeDB()
        {
            return db.Permisos
                .Where(p => p.EstaActivo)
                .OrderBy(p => p.Orden)
                .ToList();
        }

        private List<Permisos> ObtenerPermisosPorRol(string rol)
        {
            if (string.IsNullOrEmpty(rol))
                return new List<Permisos>();

            // Obtener los IDs de permisos asignados a este rol
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
