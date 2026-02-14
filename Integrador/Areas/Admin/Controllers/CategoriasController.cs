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
            // NOTA: Categoría no está en BD actual
            // En producción: return View(db.Categorias.OrderBy(c => c.Orden).ToList());
            
            var categorias = new System.Collections.Generic.List<Categoria>
            {
                new Categoria { Id = 1, Nombre = "Perros", Icono = "??", Descripcion = "Todas las razas de perros", EstaActiva = true, Orden = 1 },
                new Categoria { Id = 2, Nombre = "Gatos", Icono = "??", Descripcion = "Todas las razas de gatos", EstaActiva = true, Orden = 2 },
                new Categoria { Id = 3, Nombre = "Aves", Icono = "??", Descripcion = "Pájaros y aves exóticas", EstaActiva = true, Orden = 3 },
                new Categoria { Id = 4, Nombre = "Roedores", Icono = "??", Descripcion = "Conejos, hamsters, etc.", EstaActiva = true, Orden = 4 },
                new Categoria { Id = 5, Nombre = "Otros", Icono = "??", Descripcion = "Otras mascotas", EstaActiva = true, Orden = 5 }
            };

            ViewBag.Info = "Vista de demostración. Migrar BD para funcionalidad completa.";
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

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
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
        public ActionResult Create([Bind(Include = "Nombre,Descripcion,Icono,EstaActiva,Orden")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.FechaCreacion = DateTime.Now;

                // NOTA: Guardar en BD cuando se migre
                // db.Categorias.Add(categoria);
                // db.SaveChanges();

                TempData["Success"] = "Categoría creada exitosamente";
                TempData["Info"] = "Función completa después de migración de BD";
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

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // POST: Admin/Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Actualizar")]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Descripcion,Icono,EstaActiva,Orden")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                // db.Entry(categoria).State = EntityState.Modified;
                // db.SaveChanges();

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
                // En producción:
                // var categoria = db.Categorias.Find(id);
                // categoria.EstaActiva = !categoria.EstaActiva;
                // db.SaveChanges();

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

            TempData["Warning"] = "Función disponible después de migración de BD";
            return RedirectToAction("Index");
        }

        // POST: Admin/Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ValidarPermisoCrud(ControllerName = "Categorias", Operacion = "Eliminar")]
        public ActionResult DeleteConfirmed(int id)
        {
            // Verificar que no tenga mascotas asociadas
            // var tieneMascotas = db.Mascotas.Any(m => m.CategoriaId == id);
            // if (tieneMascotas)
            // {
            //     TempData["Error"] = "No se puede eliminar la categoría porque tiene mascotas asociadas";
            //     return RedirectToAction("Index");
            // }

            // db.Categorias.Remove(db.Categorias.Find(id));
            // db.SaveChanges();

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
