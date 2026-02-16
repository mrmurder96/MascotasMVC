using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Filters;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class RefugiosController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Refugios
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Leer")]
        public ActionResult Index(string buscar, bool? soloActivos)
        {
            var refugios = db.Refugios.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                refugios = refugios.Where(r => r.EstaActivo);
            }

            if (!string.IsNullOrEmpty(buscar))
            {
                refugios = refugios.Where(r =>
                    r.Nombre.Contains(buscar) ||
                    r.Direccion.Contains(buscar) ||
                    r.NombreResponsable.Contains(buscar));
            }

            ViewBag.SoloActivos = soloActivos;
            ViewBag.Buscar = buscar;

            return View(refugios.OrderBy(r => r.Nombre).ToList());
        }

        // GET: Admin/Refugios/Details/5
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Refugios refugio = db.Refugios.Find(id);
            if (refugio == null)
            {
                return HttpNotFound();
            }

            return View(refugio);
        }

        // GET: Admin/Refugios/Create
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Crear")]
        public ActionResult Create()
        {
            return View(new Refugios { EstaActivo = true });
        }

        // POST: Admin/Refugios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Crear")]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,Direccion,Telefono,Email,NombreResponsable,Latitud,Longitud,SitioWeb,CapacidadMaxima,MascotasActuales,EstaActivo")] Refugios refugio)
        {
            if (ModelState.IsValid)
            {
                refugio.FechaRegistro = DateTime.Now;

                if (refugio.MascotasActuales > refugio.CapacidadMaxima)
                {
                    ModelState.AddModelError("MascotasActuales", "Las mascotas actuales no pueden superar la capacidad máxima");
                    return View(refugio);
                }

                db.Refugios.Add(refugio);
                db.SaveChanges();

                TempData["Success"] = "Refugio registrado exitosamente";
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

            Refugios refugio = db.Refugios.Find(id);
            if (refugio == null)
            {
                return HttpNotFound();
            }

            return View(refugio);
        }

        // POST: Admin/Refugios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Actualizar")]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Descripcion,Direccion,Telefono,Email,NombreResponsable,Latitud,Longitud,SitioWeb,CapacidadMaxima,MascotasActuales,EstaActivo,FechaRegistro")] Refugios refugio)
        {
            if (ModelState.IsValid)
            {
                if (refugio.MascotasActuales > refugio.CapacidadMaxima)
                {
                    ModelState.AddModelError("MascotasActuales", "Las mascotas actuales no pueden superar la capacidad máxima");
                    return View(refugio);
                }

                db.Entry(refugio).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Refugio actualizado exitosamente";
                return RedirectToAction("Index");
            }

            return View(refugio);
        }

        // GET: Admin/Refugios/Delete/5
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Refugios refugio = db.Refugios.Find(id);
            if (refugio == null)
            {
                return HttpNotFound();
            }

            ViewBag.TieneMascotas = db.Mascotas.Any(m => m.RefugioId == id);
            return View(refugio);
        }

        // POST: Admin/Refugios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Refugios", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            var refugio = db.Refugios.Find(id);

            if (db.Mascotas.Any(m => m.RefugioId == id))
            {
                TempData["Error"] = "No se puede eliminar un refugio con mascotas asociadas";
                return RedirectToAction("Index");
            }

            db.Refugios.Remove(refugio);
            db.SaveChanges();

            TempData["Success"] = "Refugio eliminado exitosamente";
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
