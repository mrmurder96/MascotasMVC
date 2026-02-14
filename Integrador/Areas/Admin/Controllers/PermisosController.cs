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
            // Obtener todos los permisos con información CRUD
            var permisosDisponibles = ObtenerPermisosDisponibles();
            
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
                var key = $"Permiso_{rol}_{permisoId}_Acceso";
                Session[key] = tieneAcceso;

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
                // Guardar en sesión (en producción sería en BD)
                var key = $"PermisoCrud_{rol}_{controllerName}_{operacion}";
                Session[key] = tienePermiso;

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

            foreach (var permiso in permisos)
            {
                var vm = new PermisoConCrudViewModel
                {
                    Permiso = permiso,
                    TieneAcceso = TieneAccesoPermiso(rol, permiso.Id),
                    PermisoCrud = new PermisoCrud
                    {
                        PermisoId = permiso.Id,
                        Rol = rol,
                        ControllerName = permiso.ControllerName,
                        PuedeCrear = ObtenerPermisoCrud(rol, permiso.ControllerName, "Crear"),
                        PuedeLeer = ObtenerPermisoCrud(rol, permiso.ControllerName, "Leer"),
                        PuedeActualizar = ObtenerPermisoCrud(rol, permiso.ControllerName, "Actualizar"),
                        PuedeEliminar = ObtenerPermisoCrud(rol, permiso.ControllerName, "Eliminar")
                    }
                };

                result.Add(vm);
            }

            return result;
        }

        private bool TieneAccesoPermiso(string rol, int permisoId)
        {
            if (rol == "Administrador")
                return true;

            var key = $"Permiso_{rol}_{permisoId}_Acceso";
            var valor = Session[key];

            if (valor != null && valor is bool)
                return (bool)valor;

            // Por defecto, ciudadanos tienen acceso a: Centros, Perfil, Notificaciones
            if (rol == "Ciudadano")
            {
                return permisoId == 6 || permisoId == 7 || permisoId == 8;
            }

            return false;
        }

        private bool ObtenerPermisoCrud(string rol, string controllerName, string operacion)
        {
            if (rol == "Administrador")
                return true; // Admin puede hacer todo

            var key = $"PermisoCrud_{rol}_{controllerName}_{operacion}";
            var valor = Session[key];

            if (valor != null && valor is bool)
                return (bool)valor;

            // Por defecto, solo permitir Leer
            return operacion == "Leer";
        }

        private List<Permisos> ObtenerPermisosDisponibles()
        {
            return new List<Permisos>
            {
                new Permisos { Id = 1, Nombre = "Dashboard", ControllerName = "Admin", ActionName = "Index", Icono = "??", Orden = 1, EstaActivo = true, TieneCrud = false },
                new Permisos { Id = 2, Nombre = "Usuarios", ControllerName = "Usuarios", ActionName = "Index", Icono = "??", Orden = 2, EstaActivo = true, TieneCrud = true },
                new Permisos { Id = 3, Nombre = "Mascotas", ControllerName = "Mascotas", ActionName = "Index", Icono = "??", Orden = 3, EstaActivo = true, TieneCrud = true },
                new Permisos { Id = 4, Nombre = "Campañas", ControllerName = "Campanas", ActionName = "Index", Icono = "??", Orden = 4, EstaActivo = true, TieneCrud = true },
                new Permisos { Id = 5, Nombre = "Adopciones", ControllerName = "Adopciones", ActionName = "Index", Icono = "??", Orden = 5, EstaActivo = true, TieneCrud = true },
                new Permisos { Id = 6, Nombre = "Centros", ControllerName = "Centros", ActionName = "Index", Icono = "??", Orden = 6, EstaActivo = true, TieneCrud = false },
                new Permisos { Id = 7, Nombre = "Perfil", ControllerName = "Ciudadano", ActionName = "Perfil", Icono = "??", Orden = 7, EstaActivo = true, TieneCrud = false },
                new Permisos { Id = 8, Nombre = "Notificaciones", ControllerName = "Ciudadano", ActionName = "Notificaciones", Icono = "??", Orden = 8, EstaActivo = true, TieneCrud = false },
                new Permisos { Id = 9, Nombre = "Permisos", ControllerName = "Permisos", ActionName = "Index", Icono = "??", Orden = 9, EstaActivo = true, TieneCrud = false }
            };
        }

        private List<Permisos> ObtenerPermisosPorRol(string rol)
        {
            var todosPermisos = ObtenerPermisosDisponibles();

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
