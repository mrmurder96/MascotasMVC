using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;

namespace Integrador.Controllers
{
    [CargarPermisos]
    public class CampanasController : Controller
    {
        private adopEntities db = new adopEntities();

        public ActionResult Index()
        {
            var campanias = db.Campanias
                .Where(c => c.Activa == true)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();

            var lista = campanias.Select(c => new Campania
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                FechaInicio = c.FechaInicio,
                FechaFin = c.FechaFin,
                Activa = c.Activa,
                ImagenUrl = c.ImagenUrl
            }).ToList();

            return View(lista);
        }

        [Authorize]
        public ActionResult MisCampanas()
        {
            var campanias = db.Campanias
                .Where(c => c.Activa == true)
                .OrderByDescending(c => c.FechaInicio)
                .ToList();

            var lista = campanias.Select(c => new Campania
            {
                Id = c.Id,
                Titulo = c.Titulo,
                Descripcion = c.Descripcion,
                FechaInicio = c.FechaInicio,
                FechaFin = c.FechaFin,
                Activa = c.Activa,
                ImagenUrl = c.ImagenUrl
            }).ToList();

            return View(lista);
        }

        public ActionResult Details(int id)
        {
            var campania = db.Campanias.Find(id);
            if (campania == null)
                return HttpNotFound();

            var c = new Campania
            {
                Id = campania.Id,
                Titulo = campania.Titulo,
                Descripcion = campania.Descripcion,
                FechaInicio = campania.FechaInicio,
                FechaFin = campania.FechaFin,
                Activa = campania.Activa,
                ImagenUrl = campania.ImagenUrl
            };

            return View(c);
        }

        public ActionResult DebugSession()
        {
            return Json(new
            {
                SessionUsuarioId = Session["UsuarioId"],
                SessionNombre = Session["Nombre"],
                SessionRol = Session["Rol"],
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false
            }, JsonRequestBehavior.AllowGet);
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