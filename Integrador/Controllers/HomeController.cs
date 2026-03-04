

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Integrador.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Si el usuario ya está autenticado, redirigirlo a su dashboard correspondiente
            if (Session["UsuarioId"] != null && Session["Rol"] != null)
            {
                var rol = Session["Rol"].ToString();
                if (rol == "Administrador")
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                else if (rol == "Ciudadano")
                    return RedirectToAction("Index", "Ciudadano");
            }

            // Si no está autenticado, mostrar la página de inicio pública
            return View();
        }

        // Acción para mostrar siempre la landing page pública (sin redirecciones)
        public ActionResult Landing()
        {
            return View("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // Acción añadida para mostrar la vista de Soporte
        public ActionResult Soporte()
        {
            return View();
        }
    }
}