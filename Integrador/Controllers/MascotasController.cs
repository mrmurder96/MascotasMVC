using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;

namespace Integrador.Controllers
{
    [CargarPermisos]
    public class MascotasController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: /Mascotas - Página pública de mascotas disponibles
        public ActionResult Index()
        {
            var mascotas = db.Mascotas
                .Where(m => m.Estado == "Disponible")
                .OrderBy(m => m.Nombre)
                .ToList();

            var lista = mascotas.Select(m => new Mascota
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Tipo = m.Tipo,
                Edad = m.Edad ?? 0,
                Ubicacion = m.Ubicacion,
                Descripcion = m.Descripcion,
                FotoUrl = m.FotoUrl,
                Estado = m.Estado,
                Raza = m.Raza,
                Sexo = m.Sexo,
                Tamano = m.Tamano
            }).ToList();

            return View(lista);
        }

        // GET: /Mascotas/Details/5
        public ActionResult Details(int id)
        {
            var mascotaDb = db.Mascotas.Find(id);
            if (mascotaDb == null)
                return HttpNotFound();

            var m = new Mascota
            {
                Id = mascotaDb.Id,
                Nombre = mascotaDb.Nombre,
                Tipo = mascotaDb.Tipo,
                Edad = mascotaDb.Edad ?? 0,
                Ubicacion = mascotaDb.Ubicacion,
                Descripcion = mascotaDb.Descripcion,
                FotoUrl = mascotaDb.FotoUrl,
                Estado = mascotaDb.Estado,
                Raza = mascotaDb.Raza,
                Sexo = mascotaDb.Sexo,
                Tamano = mascotaDb.Tamano
            };

            return View(m);
        }

        // GET: /Mascotas/Adopt/5 - Redirige a login si no está autenticado
        [HttpGet]
        public ActionResult Adopt(int id)
        {
            // Si no está logueado, guardar la URL de retorno y redirigir al login
            if (Session["UsuarioId"] == null)
            {
                Session["ReturnUrl"] = Url.Action("Adopt", "Mascotas", new { id = id });
                return RedirectToAction("Login", "Account");
            }

            var mascotaDb = db.Mascotas.Find(id);
            if (mascotaDb == null || mascotaDb.Estado != "Disponible")
            {
                TempData["Error"] = "La mascota no está disponible para adopción.";
                return RedirectToAction("Index");
            }

            // Prellenar datos del usuario
            var usuarioId = Convert.ToInt32(Session["UsuarioId"]);
            var usuario = db.Usuarios.Find(usuarioId);

            var mascota = new Mascota
            {
                Id = mascotaDb.Id,
                Nombre = mascotaDb.Nombre,
                Tipo = mascotaDb.Tipo,
                Edad = mascotaDb.Edad ?? 0,
                Ubicacion = mascotaDb.Ubicacion,
                Descripcion = mascotaDb.Descripcion,
                FotoUrl = mascotaDb.FotoUrl,
                Estado = mascotaDb.Estado
            };

            if (usuario != null)
            {
                ViewBag.NombreSolicitante = usuario.Nombres + " " + usuario.Apellidos;
                ViewBag.Email = usuario.Email;
                ViewBag.Telefono = usuario.Telefono;
            }

            return View(mascota);
        }

        // POST: /Mascotas/Adopt/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adopt(int id, string nombreSolicitante, string email, string telefono)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombreSolicitante))
            {
                ModelState.AddModelError("", "Nombre y email son obligatorios.");
                var mascotaDb = db.Mascotas.Find(id);
                return View(new Mascota { Id = id, Nombre = mascotaDb?.Nombre });
            }

            var usuarioId = Convert.ToInt32(Session["UsuarioId"]);

            try
            {
                // Crear solicitud de adopción
                var adopcion = new Adopciones
                {
                    MascotaId = id,
                    UsuarioId = usuarioId,
                    NombreSolicitante = nombreSolicitante,
                    Email = email,
                    Telefono = telefono,
                    FechaSolicitud = DateTime.Now,
                    Estado = "Pendiente"
                };
                db.Adopciones.Add(adopcion);

                // Actualizar estado de mascota
                var mascota = db.Mascotas.Find(id);
                if (mascota != null)
                {
                    mascota.Estado = "En proceso";
                }

                db.SaveChanges();

                TempData["Success"] = "¡Solicitud de adopción enviada exitosamente! Nos pondremos en contacto contigo pronto.";
                return RedirectToAction("MisAdopciones", "Ciudadano");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al procesar la solicitud: " + ex.Message);
                return View(new Mascota { Id = id });
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