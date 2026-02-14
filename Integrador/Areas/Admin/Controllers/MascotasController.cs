using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para gestión completa de Mascotas (RF-09, RF-12, RF-13)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class MascotasController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Mascotas
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Leer")]
        public ActionResult Index(string buscar, string filtroEstado, string filtroTipo)
        {
            var mascotas = db.Mascotas.AsQueryable();

            // Filtro por búsqueda
            if (!string.IsNullOrEmpty(buscar))
            {
                mascotas = mascotas.Where(m => 
                    m.Nombre.Contains(buscar) || 
                    m.Descripcion.Contains(buscar) ||
                    m.Ubicacion.Contains(buscar));
            }

            // Filtro por estado
            if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "Todos")
            {
                mascotas = mascotas.Where(m => m.Estado == filtroEstado);
            }

            // Filtro por tipo
            if (!string.IsNullOrEmpty(filtroTipo) && filtroTipo != "Todos")
            {
                mascotas = mascotas.Where(m => m.Tipo == filtroTipo);
            }

            // ViewBags para los filtros
            ViewBag.Estados = new SelectList(new[] { "Todos", "Disponible", "Adoptado", "En Proceso", "No Disponible" });
            ViewBag.Tipos = new SelectList(new[] { "Todos", "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
            ViewBag.EstadoSeleccionado = filtroEstado;
            ViewBag.TipoSeleccionado = filtroTipo;
            ViewBag.Buscar = buscar;

            return View(mascotas.OrderBy(m => m.Nombre).ToList());
        }

        // GET: Admin/Mascotas/Details/5
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Mascotas mascota = db.Mascotas.Find(id);
            if (mascota == null)
            {
                return HttpNotFound();
            }

            // Obtener adopciones relacionadas
            ViewBag.Adopciones = db.Adopciones.Where(a => a.MascotaId == id).OrderByDescending(a => a.FechaSolicitud).ToList();

            return View(mascota);
        }

        // GET: Admin/Mascotas/Create
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Crear")]
        public ActionResult Create()
        {
            ViewBag.Estados = new SelectList(new[] { "Disponible", "No Disponible", "En Cuarentena", "En Tratamiento" });
            ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" });
            return View();
        }

        // POST: Admin/Mascotas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Crear")]
        public ActionResult Create([Bind(Include = "Nombre,Tipo,Edad,Ubicacion,Descripcion,Estado")] Mascotas mascota, HttpPostedFileBase foto)
        {
            if (ModelState.IsValid)
            {
                // Procesar imagen si se cargó
                if (foto != null && foto.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(foto.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Solo se permiten imágenes JPG, PNG o GIF");
                        ViewBag.Estados = new SelectList(new[] { "Disponible", "No Disponible", "En Cuarentena", "En Tratamiento" });
                        ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" });
                        return View(mascota);
                    }

                    if (foto.ContentLength > 5 * 1024 * 1024) // 5 MB
                    {
                        ModelState.AddModelError("", "La imagen no puede superar los 5 MB");
                        ViewBag.Estados = new SelectList(new[] { "Disponible", "No Disponible", "En Cuarentena", "En Tratamiento" });
                        ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" });
                        return View(mascota);
                    }

                    // Guardar imagen
                    var uploadsPath = Server.MapPath("~/Content/uploads/mascotas");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsPath, fileName);
                    foto.SaveAs(filePath);

                    mascota.FotoUrl = "/Content/uploads/mascotas/" + fileName;
                }
                else
                {
                    // Imagen por defecto
                    mascota.FotoUrl = "/Content/images/mascota-default.png";
                }

                db.Mascotas.Add(mascota);
                db.SaveChanges();

                TempData["Success"] = "Mascota registrada exitosamente";
                return RedirectToAction("Index");
            }

            ViewBag.Estados = new SelectList(new[] { "Disponible", "No Disponible", "En Cuarentena", "En Tratamiento" });
            ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" });
            return View(mascota);
        }

        // GET: Admin/Mascotas/Edit/5
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Mascotas mascota = db.Mascotas.Find(id);
            if (mascota == null)
            {
                return HttpNotFound();
            }

            ViewBag.Estados = new SelectList(new[] { "Disponible", "Adoptado", "En Proceso", "No Disponible", "En Cuarentena", "En Tratamiento" }, mascota.Estado);
            ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" }, mascota.Tipo);

            return View(mascota);
        }

        // POST: Admin/Mascotas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Actualizar")]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Tipo,Edad,Ubicacion,Descripcion,Estado,FotoUrl")] Mascotas mascota, HttpPostedFileBase foto)
        {
            if (ModelState.IsValid)
            {
                // Si se cargó una nueva imagen
                if (foto != null && foto.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(foto.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Solo se permiten imágenes JPG, PNG o GIF");
                        ViewBag.Estados = new SelectList(new[] { "Disponible", "Adoptado", "En Proceso", "No Disponible" }, mascota.Estado);
                        ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" }, mascota.Tipo);
                        return View(mascota);
                    }

                    if (foto.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "La imagen no puede superar los 5 MB");
                        ViewBag.Estados = new SelectList(new[] { "Disponible", "Adoptado", "En Proceso", "No Disponible" }, mascota.Estado);
                        ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" }, mascota.Tipo);
                        return View(mascota);
                    }

                    // Eliminar imagen anterior si no es la por defecto
                    if (!string.IsNullOrEmpty(mascota.FotoUrl) && !mascota.FotoUrl.Contains("default"))
                    {
                        var oldImagePath = Server.MapPath("~" + mascota.FotoUrl);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Guardar nueva imagen
                    var uploadsPath = Server.MapPath("~/Content/uploads/mascotas");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsPath, fileName);
                    foto.SaveAs(filePath);

                    mascota.FotoUrl = "/Content/uploads/mascotas/" + fileName;
                }

                db.Entry(mascota).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Mascota actualizada exitosamente";
                return RedirectToAction("Index");
            }

            ViewBag.Estados = new SelectList(new[] { "Disponible", "Adoptado", "En Proceso", "No Disponible" }, mascota.Estado);
            ViewBag.Tipos = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Reptil", "Otro" }, mascota.Tipo);
            return View(mascota);
        }

        // GET: Admin/Mascotas/Delete/5
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Mascotas mascota = db.Mascotas.Find(id);
            if (mascota == null)
            {
                return HttpNotFound();
            }

            // Verificar si tiene adopciones asociadas
            var tieneAdopciones = db.Adopciones.Any(a => a.MascotaId == id);
            ViewBag.TieneAdopciones = tieneAdopciones;

            return View(mascota);
        }

        // POST: Admin/Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            Mascotas mascota = db.Mascotas.Find(id);

            // Verificar si tiene adopciones (RF-21)
            var tieneAdopciones = db.Adopciones.Any(a => a.MascotaId == id);
            if (tieneAdopciones)
            {
                TempData["Error"] = "No se puede eliminar la mascota porque tiene solicitudes de adopción asociadas";
                return RedirectToAction("Index");
            }

            // Eliminar imagen física si no es la por defecto
            if (!string.IsNullOrEmpty(mascota.FotoUrl) && !mascota.FotoUrl.Contains("default"))
            {
                var imagePath = Server.MapPath("~" + mascota.FotoUrl);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            db.Mascotas.Remove(mascota);
            db.SaveChanges();

            TempData["Success"] = "Mascota eliminada exitosamente";
            return RedirectToAction("Index");
        }

        // Acción para cambiar disponibilidad rápidamente (RF-13)
        [HttpPost]
        [ValidarPermisoCrud(ControllerName = "Mascotas", Operacion = "Actualizar")]
        public JsonResult CambiarEstado(int id, string nuevoEstado)
        {
            try
            {
                var mascota = db.Mascotas.Find(id);
                if (mascota == null)
                {
                    return Json(new { success = false, message = "Mascota no encontrada" });
                }

                mascota.Estado = nuevoEstado;
                db.SaveChanges();

                return Json(new { success = true, message = "Estado actualizado" });
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
