using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para gestión de Categorías de Mascotas (RF-12)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class CategoriasController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Categorias
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Leer")]
        public ActionResult Index()
        {
            var categorias = db.Categorias.Include("Mascotas").OrderBy(c => c.Orden).ToList();
            return View(categorias);
        }

        // GET: Admin/Categorias/Details/5
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Leer")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            return View(categoria);
        }

        // GET: Admin/Categorias/Create
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Crear")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Crear")]
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,Icono,EstaActiva,Orden")] Categorias categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.FechaCreacion = DateTime.Now;
                db.Categorias.Add(categoria);
                db.SaveChanges();

                TempData["Success"] = "Categoría creada exitosamente";
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        // GET: Admin/Categorias/Edit/5
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Actualizar")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            return View(categoria);
        }

        // POST: Admin/Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Actualizar")]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Descripcion,Icono,EstaActiva,Orden")] Categorias categoria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoria).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Categoría actualizada";
                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        // POST: Cambiar estado activo/inactivo
        [HttpPost]
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Actualizar")]
        public JsonResult CambiarEstado(int id)
        {
            try
            {
                var categoria = db.Categorias.Find(id);
                if (categoria == null)
                    return Json(new { success = false, message = "Categoría no encontrada" });

                categoria.EstaActiva = !categoria.EstaActiva;
                db.SaveChanges();

                return Json(new { success = true, message = "Estado actualizado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Categorias/Delete/5
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Eliminar")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            ViewBag.TieneMascotas = db.Mascotas.Any(m => m.CategoriaId == id);
            return View(categoria);
        }

        // POST: Admin/Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            var tieneMascotas = db.Mascotas.Any(m => m.CategoriaId == id);
            if (tieneMascotas)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque tiene mascotas asociadas";
                return RedirectToAction("Index");
            }

            var categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            db.Categorias.Remove(categoria);
            db.SaveChanges();

            TempData["Success"] = "Categoría eliminada";
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
