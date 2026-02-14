using System;
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
    /// Controlador para gestión de Mascotas Perdidas/Encontradas (RF-19, RF-26)
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
            // NOTA: MascotaPerdida no está en BD actual, por ahora retornamos lista vacía
            // En producción: var reportes = db.MascotasPerdidas.AsQueryable();
            var reportes = new System.Collections.Generic.List<MascotaPerdida>();

            ViewBag.TiposReporte = new SelectList(new[] { "Todos", "Perdida", "Encontrada" });
            ViewBag.Estados = new SelectList(new[] { "Todos", "Activo", "Resuelto", "Cerrado" });
            ViewBag.TipoSeleccionado = filtroTipo;
            ViewBag.EstadoSeleccionado = filtroEstado;
            ViewBag.Buscar = buscar;

            ViewBag.TotalPerdidas = 0; // db.MascotasPerdidas.Count(m => m.TipoReporte == "Perdida" && m.Estado == "Activo");
            ViewBag.TotalEncontradas = 0; // db.MascotasPerdidas.Count(m => m.TipoReporte == "Encontrada" && m.Estado == "Activo");
            ViewBag.TotalResueltos = 0; // db.MascotasPerdidas.Count(m => m.Estado == "Resuelto");

            return View(reportes);
        }

        // GET: Admin/MascotasPerdidas/Details/5
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // En producción: var reporte = db.MascotasPerdidas.Find(id);
            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // GET: Admin/MascotasPerdidas/Create
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Crear")]
        public ActionResult Create()
        {
            ViewBag.TiposReporte = new SelectList(new[] { "Perdida", "Encontrada" });
            ViewBag.TiposMascota = new SelectList(new[] { "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
            ViewBag.Estados = new SelectList(new[] { "Activo", "Resuelto", "Cerrado" });

            var reporte = new MascotaPerdida
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
        public ActionResult Create(MascotaPerdida reporte, HttpPostedFileBase foto)
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
                        ModelState.AddModelError("", "Solo se permiten imágenes JPG, PNG o GIF");
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

                    reporte.FotoUrl = "/Content/uploads/perdidas/" + fileName;
                }

                reporte.FechaReporte = DateTime.Now;

                if (Session["UsuarioId"] != null)
                {
                    reporte.UsuarioRegistraId = Convert.ToInt32(Session["UsuarioId"]);
                }

                // NOTA: Guardar en BD cuando se migre
                // db.MascotasPerdidas.Add(reporte);
                // db.SaveChanges();

                TempData["Success"] = "Reporte registrado exitosamente";
                TempData["Info"] = "Función completa después de migración de BD";
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
                // En producción:
                // var reporte = db.MascotasPerdidas.Find(id);
                // reporte.Estado = "Resuelto";
                // reporte.FechaResolucion = DateTime.Now;
                // reporte.Notas = notas;
                // db.SaveChanges();

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

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // GET: Admin/MascotasPerdidas/Delete/5
        [ValidarPermisoCrud(ControllerName = "MascotasPerdidas", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TempData["Warning"] = "Función disponible después de migración de BD";
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
