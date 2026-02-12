using Integrador.Filters;
using System.Web.Mvc;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
