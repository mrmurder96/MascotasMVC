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
public ActionResult Adopt(int id, FormCollection form)
{
    if (Session["UsuarioId"] == null)
        return RedirectToAction("Login", "Account");

    var usuarioId = Convert.ToInt32(Session["UsuarioId"]);

    try
    {
        var adopcion = new Adopciones
        {
            MascotaId = id,
            UsuarioId = usuarioId,
            NombreSolicitante = form["nombreSolicitante"],
            Email = form["email"],
            Telefono = form["telefono"],
            FechaSolicitud = DateTime.Now,
            Estado = "Pendiente",
            Documento = form["documento"],
            Direccion = form["direccion"],
            Ciudad = form["ciudad"],
            CodigoPostal = form["codigoPostal"],
            TipoVivienda = form["tipoVivienda"],
            PropietarioInquilino = form["propietarioInquilino"],
            PermiteMascotas = form["permiteMascotas"],
            TienePatio = form["tienePatio"],
            PersonasHogar = form["personasHogar"],
            HayNinos = form["hayNinos"],
            EdadesNinos = form["edadesNinos"],
            ExperienciaMascotas = form["experienciaMascotas"],
            QuePasoMascotas = form["quepasoMascotas"],
            OtrasMascotas = form["otrasMascotas"],
            DescripcionOtrasMascotas = form["descripcionOtrasMascotas"],
            HorasSola = form["horasSola"],
            LugarDormir = form["lugardormir"],
            AccesoVeterinario = form["accesoVeterinario"],
            GastosVeterinarios = form["gastosVeterinarios"],
            PlanMudanza = form["planMudanza"],
            Motivacion = form["motivacion"],
            ComoSeEntero = form["comoSeEntero"],
            AceptaTerminos = form["aceptaTerminos"] == "on" || form["aceptaTerminos"] == "true",
            AceptaVisitas = form["aceptaVisitas"] == "on" || form["aceptaVisitas"] == "true"
        };
        db.Adopciones.Add(adopcion);

        var mascota = db.Mascotas.Find(id);
        if (mascota != null)
        {
            mascota.Estado = "En proceso";
        }

        db.SaveChanges();

        TempData["Success"] = "Solicitud de adopción enviada exitosamente! Nos pondremos en contacto contigo pronto.";
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