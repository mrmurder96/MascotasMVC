using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Helpers;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para Seguimiento Post-Adopción (RF-18)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class SeguimientosController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Seguimientos
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Leer")]
        public ActionResult Index(string filtroEstado, DateTime? fechaDesde)
        {
            // Obtener adopciones aprobadas/completadas
            var adopcionesAprobadas = db.Adopciones
                .Include(a => a.Mascotas)
                .Where(a => a.Estado == "Aprobada" || a.Estado == "Completada")
                .OrderByDescending(a => a.FechaSolicitud)
                .ToList();

            // Filtros
            if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "Todos")
            {
                if (filtroEstado == "Pendiente")
                {
                    // Adopciones sin seguimiento en los últimos 30 días
                    adopcionesAprobadas = adopcionesAprobadas
                        .Where(a => !TieneSeguimientoReciente(a.Id, 30))
                        .ToList();
                }
            }

            if (fechaDesde.HasValue)
            {
                adopcionesAprobadas = adopcionesAprobadas
                    .Where(a => a.FechaSolicitud >= fechaDesde.Value)
                    .ToList();
            }

            ViewBag.Estados = new SelectList(new[] { "Todos", "Pendiente", "Con Seguimiento" });
            ViewBag.EstadoSeleccionado = filtroEstado;
            ViewBag.FechaDesde = fechaDesde;

            return View(adopcionesAprobadas);
        }

        // GET: Admin/Seguimientos/Details/5 (Ver seguimientos de una adopción)
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var adopcion = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == id);
            if (adopcion == null)
            {
                return HttpNotFound();
            }

            ViewBag.Seguimientos = db.Seguimientos.Where(s => s.AdopcionId == id).OrderByDescending(s => s.FechaSeguimiento).ToList();

            return View(adopcion);
        }

        // GET: Admin/Seguimientos/Create?adopcionId=5
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Crear")]
        public ActionResult Create(int? adopcionId)
        {
            if (adopcionId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var adopcion = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == adopcionId);
            if (adopcion == null)
            {
                return HttpNotFound();
            }

            ViewBag.Adopcion = adopcion;
            ViewBag.TiposSeguimiento = new SelectList(new[] { "Visita Domiciliaria", "Llamada Telefónica", "Videollamada", "Email" });
            ViewBag.EstadosMascota = new SelectList(new[] { "Excelente", "Bueno", "Regular", "Requiere Atención", "Crítico" });

            var seguimiento = new Seguimientos
            {
                AdopcionId = adopcionId.Value,
                FechaSeguimiento = DateTime.Now
            };

            return View(seguimiento);
        }

        // POST: Admin/Seguimientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Crear")]
        public ActionResult Create(Seguimientos seguimiento)
        {
            if (ModelState.IsValid)
            {
                seguimiento.FechaCreacion = DateTime.Now;

                // Obtener usuario de sesión
                if (Session["UsuarioId"] != null)
                {
                    seguimiento.UsuarioRealizaSeguimientoId = Convert.ToInt32(Session["UsuarioId"]);
                }

                db.Seguimientos.Add(seguimiento);
                db.SaveChanges();

                // Notificar al adoptante sobre el seguimiento
                var adopcionNotif = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == seguimiento.AdopcionId);
                if (adopcionNotif != null && adopcionNotif.UsuarioId.HasValue)
                {
                    NotificacionHelper.NotificarNuevoSeguimiento(
                        db, 
                        adopcionNotif.UsuarioId.Value, 
                        adopcionNotif.Mascotas?.Nombre ?? "tu mascota",
                        seguimiento.EstadoMascota,
                        seguimiento.TipoSeguimiento);
                }

                TempData["Success"] = "Seguimiento registrado exitosamente";
                return RedirectToAction("Details", new { id = seguimiento.AdopcionId });
            }

            var adopcion = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == seguimiento.AdopcionId);
            ViewBag.Adopcion = adopcion;
            ViewBag.TiposSeguimiento = new SelectList(new[] { "Visita Domiciliaria", "Llamada Telefónica", "Videollamada", "Email" });
            ViewBag.EstadosMascota = new SelectList(new[] { "Excelente", "Bueno", "Regular", "Requiere Atención", "Crítico" });

            return View(seguimiento);
        }

        // GET: Admin/Seguimientos/Edit/5
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var seguimiento = db.Seguimientos.Find(id);
            if (seguimiento == null)
            {
                return HttpNotFound();
            }

            var adopcion = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == seguimiento.AdopcionId);
            ViewBag.Adopcion = adopcion;
            ViewBag.TiposSeguimiento = new SelectList(new[] { "Visita Domiciliaria", "Llamada Telefónica", "Videollamada", "Email" }, seguimiento.TipoSeguimiento);
            ViewBag.EstadosMascota = new SelectList(new[] { "Excelente", "Bueno", "Regular", "Requiere Atención", "Crítico" }, seguimiento.EstadoMascota);

            return View(seguimiento);
        }

        // POST: Admin/Seguimientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Actualizar")]
        public ActionResult Edit(Seguimientos seguimiento)
        {
            if (ModelState.IsValid)
            {
                db.Entry(seguimiento).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Seguimiento actualizado exitosamente";
                return RedirectToAction("Details", new { id = seguimiento.AdopcionId });
            }

            var adopcion = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == seguimiento.AdopcionId);
            ViewBag.Adopcion = adopcion;
            ViewBag.TiposSeguimiento = new SelectList(new[] { "Visita Domiciliaria", "Llamada Telefónica", "Videollamada", "Email" }, seguimiento.TipoSeguimiento);
            ViewBag.EstadosMascota = new SelectList(new[] { "Excelente", "Bueno", "Regular", "Requiere Atención", "Crítico" }, seguimiento.EstadoMascota);

            return View(seguimiento);
        }

        // Método auxiliar para verificar si hay seguimiento reciente
        private bool TieneSeguimientoReciente(int adopcionId, int dias)
        {
            var fechaLimite = DateTime.Now.AddDays(-dias);
            return db.Seguimientos.Any(s => s.AdopcionId == adopcionId && s.FechaSeguimiento >= fechaLimite);
        }

        // GET: Admin/Seguimientos/Delete/5
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var seguimiento = db.Seguimientos.Include("Adopciones").Include("Adopciones.Mascotas").FirstOrDefault(s => s.Id == id);
            if (seguimiento == null)
            {
                return HttpNotFound();
            }

            return View(seguimiento);
        }

        // POST: Admin/Seguimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Seguimientos", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            var seguimiento = db.Seguimientos.Find(id);
            if (seguimiento == null)
            {
                return HttpNotFound();
            }

            var adopcionId = seguimiento.AdopcionId;
            db.Seguimientos.Remove(seguimiento);
            db.SaveChanges();

            TempData["Success"] = "Seguimiento eliminado exitosamente";
            return RedirectToAction("Details", new { id = adopcionId });
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
