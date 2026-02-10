using System.IO;
using System.Web.Mvc;
using Integrador.Models.ViewModels;

namespace Integrador.Controllers
{
    public class CuentaController : Controller
    {
        [HttpGet]
        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Fotos != null)
            {
                foreach (var foto in model.Fotos)
                {
                    if (foto != null && foto.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(foto.FileName);
                        var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                        foto.SaveAs(path);
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}