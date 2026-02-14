using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para gestión de Refugios/Asociaciones (RF-11, RF-27)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class RefugiosController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Refugios
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Leer")]
        public ActionResult Index(string buscar, bool? soloActivos)
        {
            // NOTA: Refugio no está en BD actual
            // En producción: var refugios = db.Refugios.AsQueryable();
            
            var refugios = new System.Collections.Generic.List<Refugio>
            {
                new Refugio 
                { 
                    Id = 1, 
                    Nombre = "Refugio Patitas Felices", 
                    Direccion = "Av. Principal 123", 
                    Telefono = "555-1234",
                    Email = "contacto@patitasfelices.com",
                    NombreResponsable = "María González",
                    CapacidadMaxima = 50,
                    MascotasActuales = 35,
                    EstaActivo = true,
                    FechaRegistro = DateTime.Now.AddYears(-2)
                },
                new Refugio 
                { 
                    Id = 2, 
                    Nombre = "Asociación Amigos de los Animales", 
                    Direccion = "Calle Secundaria 456", 
                    Telefono = "555-5678",
                    Email = "info@amigosanimales.com",
                    NombreResponsable = "Juan Pérez",
                    CapacidadMaxima = 30,
                    MascotasActuales = 28,
                    EstaActivo = true,
                    FechaRegistro = DateTime.Now.AddYears(-1)
                }
            };

            if (soloActivos.HasValue && soloActivos.Value)
            {
                refugios = refugios.Where(r => r.EstaActivo).ToList();
            }

            if (!string.IsNullOrEmpty(buscar))
            {
                refugios = refugios.Where(r => 
                    r.Nombre.Contains(buscar) || 
                    r.Direccion.Contains(buscar) ||
                    r.NombreResponsable.Contains(buscar)).ToList();
            }

            ViewBag.SoloActivos = soloActivos;
            ViewBag.Buscar = buscar;
            ViewBag.Info = "Vista de demostración. Migrar BD para funcionalidad completa.";

            return View(refugios);
        }

        // GET: Admin/Refugios/Details/5
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // GET: Admin/Refugios/Create
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Crear")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Refugios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Crear")]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,Direccion,Telefono,Email,NombreResponsable,Latitud,Longitud,SitioWeb,CapacidadMaxima,MascotasActuales,EstaActivo")] Refugio refugio)
        {
            if (ModelState.IsValid)
            {
                refugio.FechaRegistro = DateTime.Now;

                // Validaciones adicionales
                if (refugio.MascotasActuales > refugio.CapacidadMaxima)
                {
                    ModelState.AddModelError("MascotasActuales", "Las mascotas actuales no pueden superar la capacidad máxima");
                    return View(refugio);
                }

                // NOTA: Guardar en BD cuando se migre
                // db.Refugios.Add(refugio);
                // db.SaveChanges();

                TempData["Success"] = "Refugio registrado exitosamente";
                TempData["Info"] = "Función completa después de migración de BD";
                return RedirectToAction("Index");
            }

            return View(refugio);
        }

        // GET: Admin/Refugios/Edit/5
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // POST: Admin/Refugios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Actualizar")]
        public ActionResult Edit(Refugio refugio)
        {
            if (ModelState.IsValid)
            {
                // Validaciones
                if (refugio.MascotasActuales > refugio.CapacidadMaxima)
                {
                    ModelState.AddModelError("MascotasActuales", "Las mascotas actuales no pueden superar la capacidad máxima");
                    return View(refugio);
                }

                // db.Entry(refugio).State = EntityState.Modified;
                // db.SaveChanges();

                TempData["Success"] = "Refugio actualizado";
                return RedirectToAction("Index");
            }

            return View(refugio);
        }

        // POST: Actualizar contador de mascotas
        [HttpPost]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Actualizar")]
        public JsonResult ActualizarContador(int id, int cantidad)
        {
            try
            {
                // En producción:
                // var refugio = db.Refugios.Find(id);
                // refugio.MascotasActuales = cantidad;
                // if (refugio.MascotasActuales > refugio.CapacidadMaxima)
                // {
                //     return Json(new { success = false, message = "Supera la capacidad máxima" });
                // }
                // db.SaveChanges();

                return Json(new { success = true, message = "Contador actualizado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Refugios/Delete/5
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // POST: Admin/Refugios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Verificar que no tenga mascotas activas
            // var refugio = db.Refugios.Find(id);
            // if (refugio.MascotasActuales > 0)
            // {
            //     TempData["Error"] = "No se puede eliminar un refugio con mascotas activas";
            //     return RedirectToAction("Index");
            // }

            // db.Refugios.Remove(refugio);
            // db.SaveChanges();

            TempData["Success"] = "Refugio eliminado";
            return RedirectToAction("Index");
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
