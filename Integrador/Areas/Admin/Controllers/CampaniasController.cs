using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
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
        public ActionResult Create([Bind(Include = "Id,Titulo,Descripcion,FechaInicio,FechaFin,Activa")] Campanias campania, HttpPostedFileBase ImagenFile)
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
                    // Procesar imagen si se cargó
                    if (ImagenFile != null && ImagenFile.ContentLength > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(ImagenFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("", "Solo se permiten imágenes JPG, PNG o GIF");
                            return View(campania);
                        }

                        if (ImagenFile.ContentLength > 5 * 1024 * 1024) // 5 MB
                        {
                            ModelState.AddModelError("", "La imagen no puede superar los 5 MB");
                            return View(campania);
                        }

                        // Guardar imagen
                        var uploadsPath = Server.MapPath("~/Content/uploads/campanias");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(uploadsPath, fileName);
                        ImagenFile.SaveAs(filePath);

                        campania.ImagenUrl = "~/Content/uploads/campanias/" + fileName;
                    }
                    else
                    {
                        // Imagen por defecto
                        campania.ImagenUrl = "~/Content/images/campania-default.png";
                    }

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
        public ActionResult Edit([Bind(Include = "Id,Titulo,Descripcion,FechaInicio,FechaFin,Activa,ImagenUrl")] Campanias campania, HttpPostedFileBase ImagenFile)
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
                    // Obtener la campaña existente para preservar la imagen anterior si no se sube una nueva
                    var campaniaExistente = db.Campanias.AsNoTracking().FirstOrDefault(c => c.Id == campania.Id);

                    // Procesar nueva imagen si se cargó
                    if (ImagenFile != null && ImagenFile.ContentLength > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(ImagenFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("", "Solo se permiten imágenes JPG, PNG o GIF");
                            return View(campania);
                        }

                        if (ImagenFile.ContentLength > 5 * 1024 * 1024) // 5 MB
                        {
                            ModelState.AddModelError("", "La imagen no puede superar los 5 MB");
                            return View(campania);
                        }

                        // Eliminar imagen anterior si existe y no es la por defecto
                        if (campaniaExistente != null && !string.IsNullOrEmpty(campaniaExistente.ImagenUrl) && 
                            !campaniaExistente.ImagenUrl.Contains("campania-default.png"))
                        {
                            var oldImagePath = Server.MapPath(campaniaExistente.ImagenUrl);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Guardar nueva imagen
                        var uploadsPath = Server.MapPath("~/Content/uploads/campanias");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(uploadsPath, fileName);
                        ImagenFile.SaveAs(filePath);

                        campania.ImagenUrl = "~/Content/uploads/campanias/" + fileName;
                    }
                    else
                    {
                        // Mantener la imagen existente si no se subió una nueva
                        if (campaniaExistente != null)
                        {
                            campania.ImagenUrl = campaniaExistente.ImagenUrl;
                        }
                    }

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
