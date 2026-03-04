using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;
using Integrador.Helpers;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class CampaniasController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Campanias
        public ActionResult Index()
        {
            var campanias = db.Campanias.OrderByDescending(c => c.FechaInicio).ToList();
            return View(campanias);
        }

        // GET: Admin/Campanias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Campanias campania = db.Campanias.Find(id);
            if (campania == null)
            {
                return HttpNotFound();
            }
            return View(campania);
        }

        // GET: Admin/Campanias/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Campanias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Titulo,Descripcion,FechaInicio,FechaFin,Activa,ImagenUrl")] Campanias campania)
        {
            // Validación adicional de fechas
            if (campania.FechaFin.HasValue && campania.FechaFin.Value < campania.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser mayor o igual a la fecha de inicio");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Campanias.Add(campania);
                    db.SaveChanges();

                    // Notificar a todos los usuarios sobre la nueva campaña
                    if (campania.Activa)
                    {
                        NotificacionHelper.NotificarCampanaATodos(db, campania.Titulo, campania.FechaInicio);
                    }

                    TempData["Success"] = "Campaña creada exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear la campaña: " + ex.Message);
                }
            }
            return View(campania);
        }

        // GET: Admin/Campanias/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Campanias campania = db.Campanias.Find(id);
            if (campania == null)
            {
                return HttpNotFound();
            }
            return View(campania);
        }

        // POST: Admin/Campanias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Titulo,Descripcion,FechaInicio,FechaFin,Activa,ImagenUrl")] Campanias campania)
        {
            // Validación adicional de fechas
            if (campania.FechaFin.HasValue && campania.FechaFin.Value < campania.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser mayor o igual a la fecha de inicio");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(campania).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Campaña actualizada exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la campaña: " + ex.Message);
                }
            }
            return View(campania);
        }

        // GET: Admin/Campanias/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Campanias campania = db.Campanias.Find(id);
            if (campania == null)
            {
                return HttpNotFound();
            }
            return View(campania);
        }

        // POST: Admin/Campanias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Campanias campania = db.Campanias.Find(id);
                if (campania == null)
                {
                    return HttpNotFound();
                }
                db.Campanias.Remove(campania);
                db.SaveChanges();
                TempData["Success"] = "Campaña eliminada exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la campaña: " + ex.Message;
                return RedirectToAction("Index");
            }
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
