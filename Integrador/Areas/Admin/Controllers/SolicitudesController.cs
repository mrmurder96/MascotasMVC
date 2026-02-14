using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Configuration;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para gestión de Solicitudes de Adopción (RF-15, RF-16, RF-17)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class SolicitudesController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Solicitudes
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Leer")]
        public ActionResult Index(string filtroEstado, string buscar, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var solicitudes = db.Adopciones.Include(a => a.Mascotas).AsQueryable();

            // Filtro por estado
            if (!string.IsNullOrEmpty(filtroEstado) && filtroEstado != "Todos")
            {
                solicitudes = solicitudes.Where(s => s.Estado == filtroEstado);
            }

            // Filtro por búsqueda
            if (!string.IsNullOrEmpty(buscar))
            {
                solicitudes = solicitudes.Where(s =>
                    s.NombreSolicitante.Contains(buscar) ||
                    s.Email.Contains(buscar) ||
                    s.Telefono.Contains(buscar) ||
                    s.Mascotas.Nombre.Contains(buscar));
            }

            // Filtro por fechas
            if (fechaDesde.HasValue)
            {
                solicitudes = solicitudes.Where(s => s.FechaSolicitud >= fechaDesde.Value);
            }

            if (fechaHasta.HasValue)
            {
                var fechaHastaFin = fechaHasta.Value.AddDays(1).AddSeconds(-1);
                solicitudes = solicitudes.Where(s => s.FechaSolicitud <= fechaHastaFin);
            }

            // ViewBags para filtros
            ViewBag.Estados = new SelectList(new[] { "Todos", "Pendiente", "En Revisión", "Aprobada", "Rechazada", "Completada", "Cancelada" });
            ViewBag.EstadoSeleccionado = filtroEstado;
            ViewBag.Buscar = buscar;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;

            // Estadísticas
            ViewBag.TotalPendientes = db.Adopciones.Count(a => a.Estado == "Pendiente" || a.Estado == "En Revisión");
            ViewBag.TotalAprobadas = db.Adopciones.Count(a => a.Estado == "Aprobada" || a.Estado == "Completada");
            ViewBag.TotalRechazadas = db.Adopciones.Count(a => a.Estado == "Rechazada");

            return View(solicitudes.OrderByDescending(s => s.FechaSolicitud).ToList());
        }

        // GET: Admin/Solicitudes/Details/5
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var solicitud = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }

            // Si hay UsuarioId, cargar datos del usuario
            if (solicitud.UsuarioId.HasValue)
            {
                ViewBag.Usuario = db.Usuarios.Find(solicitud.UsuarioId.Value);
            }

            return View(solicitud);
        }

        // POST: Admin/Solicitudes/Aprobar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Actualizar")]
        public ActionResult Aprobar(int id, string observaciones)
        {
            try
            {
                var solicitud = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == id);
                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index");
                }

                // Validaciones de negocio (RF-16, RF-20, RF-21)
                var mascota = solicitud.Mascotas;
                
                // RF-21: Verificar que la mascota esté disponible
                if (!mascota.EstaDisponible)
                {
                    TempData["Error"] = "La mascota ya no está disponible para adopción";
                    return RedirectToAction("Details", new { id });
                }

                // RF-20: Si hay usuario registrado, verificar mayor de edad
                if (solicitud.UsuarioId.HasValue)
                {
                    var usuario = db.Usuarios.Find(solicitud.UsuarioId.Value);
                    if (usuario != null && !usuario.EsMayorDeEdad)
                    {
                        TempData["Error"] = "El adoptante debe ser mayor de edad";
                        return RedirectToAction("Details", new { id });
                    }
                }

                // Aprobar solicitud
                solicitud.Estado = Adopciones.Estados.Aprobada;
                
                // Cambiar estado de la mascota
                mascota.Estado = "Adoptado";

                // Rechazar automáticamente otras solicitudes pendientes para esta mascota
                var otrasSolicitudesPendientes = db.Adopciones
                    .Where(a => a.MascotaId == mascota.Id && 
                                a.Id != id && 
                                (a.Estado == "Pendiente" || a.Estado == "En Revisión"))
                    .ToList();

                foreach (var otra in otrasSolicitudesPendientes)
                {
                    otra.Estado = Adopciones.Estados.Rechazada;
                }

                db.SaveChanges();

                // Enviar notificación por email (RF-28)
                EnviarNotificacionAprobacion(solicitud);

                TempData["Success"] = "Solicitud aprobada exitosamente";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al aprobar solicitud: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Admin/Solicitudes/Rechazar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Actualizar")]
        public ActionResult Rechazar(int id, string motivoRechazo)
        {
            try
            {
                var solicitud = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == id);
                if (solicitud == null)
                {
                    TempData["Error"] = "Solicitud no encontrada";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["Error"] = "Debe proporcionar un motivo de rechazo";
                    return RedirectToAction("Details", new { id });
                }

                solicitud.Estado = Adopciones.Estados.Rechazada;
                db.SaveChanges();

                // Enviar notificación de rechazo
                EnviarNotificacionRechazo(solicitud, motivoRechazo);

                TempData["Success"] = "Solicitud rechazada";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al rechazar solicitud: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Cambiar estado a En Revisión
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Actualizar")]
        public ActionResult MarcarEnRevision(int id)
        {
            try
            {
                var solicitud = db.Adopciones.Find(id);
                if (solicitud == null)
                {
                    return Json(new { success = false, message = "Solicitud no encontrada" });
                }

                solicitud.Estado = Adopciones.Estados.EnRevision;
                db.SaveChanges();

                return Json(new { success = true, message = "Solicitud marcada en revisión" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Solicitudes/Delete/5
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var solicitud = db.Adopciones.Include(a => a.Mascotas).FirstOrDefault(a => a.Id == id);
            if (solicitud == null)
            {
                return HttpNotFound();
            }

            return View(solicitud);
        }

        // POST: Admin/Solicitudes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Solicitudes", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            var solicitud = db.Adopciones.Find(id);
            
            // Solo se pueden eliminar solicitudes canceladas o muy antiguas
            if (solicitud.Estado != "Cancelada" && solicitud.Estado != "Rechazada")
            {
                TempData["Error"] = "Solo se pueden eliminar solicitudes canceladas o rechazadas";
                return RedirectToAction("Index");
            }

            db.Adopciones.Remove(solicitud);
            db.SaveChanges();

            TempData["Success"] = "Solicitud eliminada";
            return RedirectToAction("Index");
        }

        // Métodos auxiliares para envío de emails (RF-28)
        private void EnviarNotificacionAprobacion(Adopciones solicitud)
        {
            try
            {
                var from = ConfigurationManager.AppSettings["MailFrom"] ?? "no-reply@adopciones.com";
                var subject = $"? Solicitud de Adopción Aprobada - {solicitud.Mascotas.Nombre}";
                
                var sb = new StringBuilder();
                sb.AppendLine($"Hola {solicitud.NombreSolicitante},");
                sb.AppendLine();
                sb.AppendLine($"¡Excelentes noticias! Tu solicitud para adoptar a {solicitud.Mascotas.Nombre} ha sido APROBADA.");
                sb.AppendLine();
                sb.AppendLine("Próximos pasos:");
                sb.AppendLine("1. Nos comunicaremos contigo en las próximas 48 horas");
                sb.AppendLine("2. Coordinaremos una visita para conocer a tu nueva mascota");
                sb.AppendLine("3. Completaremos el proceso de adopción");
                sb.AppendLine();
                sb.AppendLine("Si tienes dudas, contáctanos.");
                sb.AppendLine();
                sb.AppendLine("¡Gracias por adoptar!");

                using (var msg = new MailMessage(from, solicitud.Email, subject, sb.ToString()))
                using (var smtp = new SmtpClient())
                {
                    smtp.Send(msg);
                }
            }
            catch
            {
                // Log error pero no fallar la operación principal
            }
        }

        private void EnviarNotificacionRechazo(Adopciones solicitud, string motivo)
        {
            try
            {
                var from = ConfigurationManager.AppSettings["MailFrom"] ?? "no-reply@adopciones.com";
                var subject = $"Actualización de Solicitud de Adopción - {solicitud.Mascotas.Nombre}";

                var sb = new StringBuilder();
                sb.AppendLine($"Hola {solicitud.NombreSolicitante},");
                sb.AppendLine();
                sb.AppendLine($"Lamentamos informarte que tu solicitud para adoptar a {solicitud.Mascotas.Nombre} no ha sido aprobada en esta ocasión.");
                sb.AppendLine();
                sb.AppendLine($"Motivo: {motivo}");
                sb.AppendLine();
                sb.AppendLine("Te invitamos a explorar otras mascotas disponibles en nuestra plataforma.");
                sb.AppendLine();
                sb.AppendLine("Gracias por tu interés.");

                using (var msg = new MailMessage(from, solicitud.Email, subject, sb.ToString()))
                using (var smtp = new SmtpClient())
                {
                    smtp.Send(msg);
                }
            }
            catch
            {
                // Log error
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
