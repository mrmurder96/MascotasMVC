using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class NotificacionesController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Notificaciones
        public ActionResult Index()
        {
            var notificaciones = db.Notificaciones
                .Include(n => n.Usuarios)
                .OrderByDescending(n => n.Fecha)
                .ToList();
            return View(notificaciones);
        }

        // GET: Admin/Notificaciones/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notificaciones notificacion = db.Notificaciones
                .Include(n => n.Usuarios)
                .FirstOrDefault(n => n.Id == id);
            if (notificacion == null)
            {
                return HttpNotFound();
            }
            return View(notificacion);
        }

        // GET: Admin/Notificaciones/Create
        public ActionResult Create()
        {
            ViewBag.UsuarioId = new SelectList(db.Usuarios.Where(u => u.EstaActivo).OrderBy(u => u.Nombres), "Id", "Nombres");
            return View();
        }

        // POST: Admin/Notificaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UsuarioId,Titulo,Mensaje,Leido,Fecha")] Notificaciones notificacion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    notificacion.Fecha = DateTime.Now;
                    notificacion.Leido = false;
                    db.Notificaciones.Add(notificacion);
                    db.SaveChanges();
                    TempData["Success"] = "Notificación creada exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear la notificación: " + ex.Message);
                }
            }

            ViewBag.UsuarioId = new SelectList(db.Usuarios.Where(u => u.EstaActivo).OrderBy(u => u.Nombres), "Id", "Nombres", notificacion.UsuarioId);
            return View(notificacion);
        }

        // GET: Admin/Notificaciones/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notificaciones notificacion = db.Notificaciones.Find(id);
            if (notificacion == null)
            {
                return HttpNotFound();
            }
            ViewBag.UsuarioId = new SelectList(db.Usuarios.Where(u => u.EstaActivo).OrderBy(u => u.Nombres), "Id", "Nombres", notificacion.UsuarioId);
            return View(notificacion);
        }

        // POST: Admin/Notificaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UsuarioId,Titulo,Mensaje,Leido,Fecha")] Notificaciones notificacion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(notificacion).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Notificación actualizada exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la notificación: " + ex.Message);
                }
            }
            ViewBag.UsuarioId = new SelectList(db.Usuarios.Where(u => u.EstaActivo).OrderBy(u => u.Nombres), "Id", "Nombres", notificacion.UsuarioId);
            return View(notificacion);
        }

        // GET: Admin/Notificaciones/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Notificaciones notificacion = db.Notificaciones
                .Include(n => n.Usuarios)
                .FirstOrDefault(n => n.Id == id);
            if (notificacion == null)
            {
                return HttpNotFound();
            }
            return View(notificacion);
        }

        // POST: Admin/Notificaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Notificaciones notificacion = db.Notificaciones.Find(id);
                if (notificacion == null)
                {
                    return HttpNotFound();
                }
                db.Notificaciones.Remove(notificacion);
                db.SaveChanges();
                TempData["Success"] = "Notificación eliminada exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar la notificación: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Enviar notificación a todos los usuarios
        [HttpPost]
        public JsonResult EnviarATodos(string titulo, string mensaje)
        {
            try
            {
                var usuarios = db.Usuarios.Where(u => u.EstaActivo).ToList();
                foreach (var usuario in usuarios)
                {
                    var notificacion = new Notificaciones
                    {
                        UsuarioId = usuario.Id,
                        Titulo = titulo,
                        Mensaje = mensaje,
                        Fecha = DateTime.Now,
                        Leido = false
                    };
                    db.Notificaciones.Add(notificacion);
                }
                db.SaveChanges();
                return Json(new { success = true, message = $"Notificación enviada a {usuarios.Count} usuarios" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
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
