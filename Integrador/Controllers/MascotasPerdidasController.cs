using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Integrador.Filters;
using Integrador.Helpers;
using Integrador.Models;

namespace Integrador.Controllers
{
    [CargarPermisos]
    public class MascotasPerdidasController : Controller
    {
        private readonly adopEntities db = new adopEntities();

        // GET: MascotasPerdidas
        public ActionResult Index()
        {
            var mascotasPerdidas = db.MascotasPerdidas
                .OrderByDescending(m => m.FechaReporte)
                .ToList();

            return View(mascotasPerdidas);
        }

        // GET: MascotasPerdidas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var mascotaPerdida = db.MascotasPerdidas.Find(id);
            if (mascotaPerdida == null)
                return HttpNotFound();

            return View(mascotaPerdida);
        }

        // GET: MascotasPerdidas/Create
        [ClienteAuthorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: MascotasPerdidas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClienteAuthorize]
        public ActionResult Create([Bind(Include = "Nombre,TipoReporte,TipoMascota,Raza,UbicacionReporte,Descripcion,NombreReportante,TelefonoContacto,EmailContacto")] MascotasPerdidas mascotaPerdida)
        {
            // Eliminar validaciones para campos que se establecen en el servidor
            ModelState.Remove("FechaReporte");
            ModelState.Remove("Estado");

            if (ModelState.IsValid)
            {
                try
                {
                    // Procesar imagen si existe
                    if (Request.Files.Count > 0)
                    {
                        var file = Request.Files[0];
                        if (file != null && file.ContentLength > 0)
                        {
                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                            var extension = Path.GetExtension(file.FileName).ToLower();

                            if (!allowedExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("", "Solo se permiten imágenes (jpg, jpeg, png, gif)");
                                return View(mascotaPerdida);
                            }

                            var fileName = $"perdida_{Guid.NewGuid()}{extension}";
                            var uploadPath = Server.MapPath("~/Content/uploads/perdidas");

                            if (!Directory.Exists(uploadPath))
                                Directory.CreateDirectory(uploadPath);

                            var filePath = Path.Combine(uploadPath, fileName);
                            file.SaveAs(filePath);

                            mascotaPerdida.FotoUrl = "/Content/uploads/perdidas/" + fileName;
                        }
                    }

                    mascotaPerdida.FechaReporte = DateTime.Now;
                    mascotaPerdida.Estado = "Activo";

                    // Establecer TipoReporte si no viene del formulario
                    if (string.IsNullOrEmpty(mascotaPerdida.TipoReporte))
                    {
                        mascotaPerdida.TipoReporte = "Perdida";
                    }

                    if (Session["UsuarioId"] != null)
                    {
                        mascotaPerdida.UsuarioRegistraId = Convert.ToInt32(Session["UsuarioId"]);
                    }

                    db.MascotasPerdidas.Add(mascotaPerdida);
                    db.SaveChanges();

                    TempData["Success"] = "Reporte de mascota perdida creado exitosamente.";
                    return RedirectToAction("Details", new { id = mascotaPerdida.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                }
            }

            return View(mascotaPerdida);
        }

        // POST: MascotasPerdidas/MarcarEncontrada/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClienteAuthorize]
        public ActionResult MarcarEncontrada(int id)
        {
            var mascotaPerdida = db.MascotasPerdidas.Find(id);
            if (mascotaPerdida == null)
                return HttpNotFound();

            // Verificar que sea el usuario que reportó
            if (Session["UsuarioId"] != null)
            {
                var usuarioId = Convert.ToInt32(Session["UsuarioId"]);
                mascotaPerdida.Encontrada = true;
                mascotaPerdida.FechaEncontrada = DateTime.Now;
                db.SaveChanges();

                // Crear notificación de mascota encontrada
                NotificacionHelper.NotificarMascotaEncontrada(db, usuarioId, mascotaPerdida.Nombre ?? "tu mascota");

                TempData["Success"] = "¡Nos alegra que hayas encontrado a tu mascota!";
            }

            return RedirectToAction("Details", new { id = id });
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
