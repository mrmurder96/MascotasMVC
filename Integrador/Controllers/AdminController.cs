using Integrador.Filters;
using Integrador.Models;
using System.Linq;
using System.Web.Mvc;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class AdminController : Controller
    {
        private readonly adopEntities db = new adopEntities();

        public ActionResult Index()
        {
            // Estadísticas para el dashboard
            ViewBag.TotalMascotas = db.Mascotas.Count();
            ViewBag.MascotasDisponibles = db.Mascotas.Count(m => m.Estado == "Disponible");
            ViewBag.TotalAdopciones = db.Adopciones.Count();
            ViewBag.AdopcionesPendientes = db.Adopciones.Count(a => a.Estado == "Pendiente");
            ViewBag.TotalUsuarios = db.Usuarios.Count();
            // Usar Estado en lugar de Encontrada (que es NotMapped)
            ViewBag.MascotasPerdidas = db.MascotasPerdidas.Count(m => m.Estado == "Activo");
            
            return View();
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

