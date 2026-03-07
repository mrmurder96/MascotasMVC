using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Helpers;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para gestion de Mascotas Perdidas/Encontradas (RF-19, RF-26)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class MascotasPerdidasController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/MascotasPerdidas
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Leer")]
        public ActionResult Index(string filtroTipo, string filtroEstado, string buscar)
        {
            var reportes = db.MascotasPerdidas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroTipo) && filtroTipo != "Todos")
            {
                reportes = reportes.Where(m => m.TipoReporte == filtroTipo);
            }

            if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "Todos")
            {
                reportes = reportes.Where(m => m.Estado == filtroEstado);
            }

            if (!string.IsNullOrEmpty(buscar))
            {
                reportes = reportes.Where(m =>
                    m.Nombre.Contains(buscar) ||
                    m.Descripcion.Contains(buscar) ||
                    m.UbicacionReporte.Contains(buscar) ||
                    m.NombreReportante.Contains(buscar));
            }

            ViewBag.TiposReporte = new SelectList(new[] { "Todos", "Perdida", "Encontrada" });
            ViewBag.Estados = new SelectList(new[] { "Todos", "Activo", "Resuelto", "Cerrado" });
            ViewBag.TipoSeleccionado = filtroTipo;
            ViewBag.EstadoSeleccionado = filtroEstado;
            ViewBag.Buscar = buscar;

            ViewBag.TotalPerdidas = db.MascotasPerdidas.Count(m => m.TipoReporte == "Perdida" && m.Estado == "Activo");
            ViewBag.TotalEncontradas = db.MascotasPerdidas.Count(m => m.TipoReporte == "Encontrada" && m.Estado == "Activo");
            ViewBag.TotalResueltos = db.MascotasPerdidas.Count(m => m.Estado == "Resuelto");

            return View(reportes.OrderByDescending(m => m.FechaReporte).ToList());
        }

        // GET: Admin/MascotasPerdidas/Details/5
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reporte = db.MascotasPerdidas.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }

            return View(reporte);
        }

        // GET: Admin/MascotasPerdidas/Create
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Crear")]
        public ActionResult Create()
        {
            ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" });
            ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
            ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" });

            var reporte = new MascotasPerdidas
            {
                FechaReporte = DateTime.Now,
                Estado = "Activo"
            };

            return View(reporte);
        }

        // POST: Admin/MascotasPerdidas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Crear")]
        public ActionResult Create(MascotasPerdidas reporte, HttpPostedFileBase foto)
        {
            if (ModelState.IsValid)
            {
                // Procesar imagen
                if (foto != null && foto.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(foto.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("", "Solo se permiten imagenes JPG, PNG o GIF");
                        ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" });
                        ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
                        ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" });
                        return View(reporte);
                    }

                    if (foto.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "La imagen no puede superar los 5 MB");
                        ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" });
                        ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
                        ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" });
                        return View(reporte);
                    }

                    var uploadsPath = Server.MapPath("~/Content/uploads/perdidas");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsPath, fileName);
                    foto.SaveAs(filePath);

                    reporte.FotoUrl = "~/Content/uploads/perdidas/" + fileName;
                }

                reporte.FechaReporte = DateTime.Now;

                if (Session["UsuarioId"] != null)
                {
                    reporte.UsuarioRegistraId = Convert.ToInt32(Session["UsuarioId"]);
                }

                db.MascotasPerdidas.Add(reporte);
                db.SaveChanges();

                TempData["Success"] = "Reporte registrado exitosamente";
                return RedirectToAction("Index");
            }

            ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" });
            ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
            ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" });

            return View(reporte);
        }

        // POST: Marcar como resuelto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Actualizar")]
        public ActionResult MarcarResuelto(int id, string notas)
        {
            try
            {
                var reporte = db.MascotasPerdidas.Find(id);
                if (reporte == null)
                {
                    TempData["Error"] = "Reporte no encontrado";
                    return RedirectToAction("Index");
                }

                reporte.Estado = "Resuelto";
                reporte.FechaResolucion = DateTime.Now;
                reporte.Notas = notas;
                db.SaveChanges();

                // Notificar al usuario que reportó
                if (reporte.UsuarioRegistraId.HasValue)
                {
                    NotificacionHelper.NotificarMascotaEncontrada(db, reporte.UsuarioRegistraId.Value, reporte.Nombre ?? "la mascota");
                }

                TempData["Success"] = "Reporte marcado como resuelto";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/MascotasPerdidas/Edit/5
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reporte = db.MascotasPerdidas.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }

            ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" }, reporte.TipoReporte);
            ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" }, reporte.TipoMascota);
            ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" }, reporte.Estado);

            return View(reporte);
        }

        // POST: Admin/MascotasPerdidas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Actualizar")]
        public ActionResult Edit(MascotasPerdidas reporte, HttpPostedFileBase foto)
        {
            if (ModelState.IsValid)
            {
                if (foto != null && foto.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(foto.FileName).ToLower();

                    if (allowedExtensions.Contains(extension) && foto.ContentLength <= 5 * 1024 * 1024)
                    {
                        var uploadsPath = Server.MapPath("~/Content/uploads/perdidas");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + extension;
                        var filePath = Path.Combine(uploadsPath, fileName);
                        foto.SaveAs(filePath);

                        reporte.FotoUrl = "~/Content/uploads/perdidas/" + fileName;
                    }
                }

                db.Entry(reporte).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Reporte actualizado exitosamente";
                return RedirectToAction("Index");
            }

            ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" }, reporte.TipoReporte);
            ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" }, reporte.TipoMascota);
            ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" }, reporte.Estado);

            return View(reporte);
        }

        // GET: Admin/MascotasPerdidas/Delete/5
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var reporte = db.MascotasPerdidas.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }

            return View(reporte);
        }

        // POST: Admin/MascotasPerdidas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            var reporte = db.MascotasPerdidas.Find(id);
            if (reporte == null)
            {
                return HttpNotFound();
            }

            db.MascotasPerdidas.Remove(reporte);
            db.SaveChanges();

            TempData["Success"] = "Reporte eliminado exitosamente";
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
