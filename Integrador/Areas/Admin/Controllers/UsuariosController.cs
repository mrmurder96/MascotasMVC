using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;
using Integrador.Helpers;

namespace Integrador.Areas.Admin.Controllers
{
    [AdminAuthorize]
    [CargarPermisos]
    public class UsuariosController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Usuarios
        public ActionResult Index()
        {
            var usuarios = db.Usuarios.OrderBy(u => u.Nombres).ToList();
            return View(usuarios);
        }

        // GET: Admin/Usuarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // GET: Admin/Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" });
            return View();
        }

        // POST: Admin/Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Nombres,Apellidos,Email,Cedula,Telefono,Direccion,Rol,FechaNacimiento,EstaActivo")] Usuarios usuario, string Password)
        {
            // Validación adicional de fecha de nacimiento
            if (usuario.FechaNacimiento.HasValue)
            {
                if (usuario.FechaNacimiento.Value > DateTime.Today)
                {
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura");
                }
                else if (usuario.FechaNacimiento.Value < DateTime.Today.AddYears(-120))
                {
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser mayor a 120 años atrás");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que el email no exista
                    if (db.Usuarios.Any(u => u.Email == usuario.Email))
                    {
                        ModelState.AddModelError("Email", "Ya existe un usuario con este correo electrónico");
                        ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
                        return View(usuario);
                    }

                    // Validar que la cédula no exista
                    if (db.Usuarios.Any(u => u.Cedula == usuario.Cedula))
                    {
                        ModelState.AddModelError("Cedula", "Ya existe un usuario con esta cédula");
                        ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
                        return View(usuario);
                    }

                    // Generar salt y hash de contraseña
                    string claveAes = ConfigurationManager.AppSettings["ClaveAES"];
                    string salt = Guid.NewGuid().ToString();
                    usuario.Salt = salt;
                    usuario.ClaveHash = AesEncryption.Encrypt(Password, claveAes);
                    usuario.FechaRegistro = DateTime.Now;
                    usuario.FechaUltimoCambioPassword = DateTime.Now;
                    usuario.IntentosFallidos = 0;
                    usuario.Bloqueado = false;
                    usuario.EstaActivo = true;

                    db.Usuarios.Add(usuario);
                    db.SaveChanges();

                    TempData["Success"] = "Usuario creado exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el usuario: " + ex.Message);
                }
            }

            ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
            return View(usuario);
        }

        // GET: Admin/Usuarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
            return View(usuario);
        }

        // POST: Admin/Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombres,Apellidos,Email,Cedula,Telefono,Direccion,Rol,FechaNacimiento,EstaActivo,Bloqueado,IntentosFallidos")] Usuarios usuario)
        {
            // Validación adicional de fecha de nacimiento
            if (usuario.FechaNacimiento.HasValue)
            {
                if (usuario.FechaNacimiento.Value > DateTime.Today)
                {
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser futura");
                }
                else if (usuario.FechaNacimiento.Value < DateTime.Today.AddYears(-120))
                {
                    ModelState.AddModelError("FechaNacimiento", "La fecha de nacimiento no puede ser mayor a 120 años atrás");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioExistente = db.Usuarios.Find(usuario.Id);
                    if (usuarioExistente == null)
                    {
                        return HttpNotFound();
                    }

                    // Validar email único
                    if (db.Usuarios.Any(u => u.Email == usuario.Email && u.Id != usuario.Id))
                    {
                        ModelState.AddModelError("Email", "Ya existe otro usuario con este correo electrónico");
                        ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
                        return View(usuario);
                    }

                    // Validar cédula única
                    if (db.Usuarios.Any(u => u.Cedula == usuario.Cedula && u.Id != usuario.Id))
                    {
                        ModelState.AddModelError("Cedula", "Ya existe otro usuario con esta cédula");
                        ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
                        return View(usuario);
                    }

                    // Actualizar solo los campos editables
                    usuarioExistente.Nombres = usuario.Nombres;
                    usuarioExistente.Apellidos = usuario.Apellidos;
                    usuarioExistente.Email = usuario.Email;
                    usuarioExistente.Cedula = usuario.Cedula;
                    usuarioExistente.Telefono = usuario.Telefono;
                    usuarioExistente.Direccion = usuario.Direccion;
                    usuarioExistente.Rol = usuario.Rol;
                    usuarioExistente.FechaNacimiento = usuario.FechaNacimiento;
                    usuarioExistente.EstaActivo = usuario.EstaActivo;
                    usuarioExistente.Bloqueado = usuario.Bloqueado;
                    usuarioExistente.IntentosFallidos = usuario.IntentosFallidos;

                    db.Entry(usuarioExistente).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Usuario actualizado exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el usuario: " + ex.Message);
                }
            }
            ViewBag.Roles = new SelectList(new[] { "Administrador", "Ciudadano" }, usuario.Rol);
            return View(usuario);
        }

        // GET: Admin/Usuarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Admin/Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Usuarios usuario = db.Usuarios.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                // No permitir eliminar el propio usuario
                var currentUserId = Session["UserId"];
                if (currentUserId != null && (int)currentUserId == id)
                {
                    TempData["Error"] = "No puedes eliminar tu propio usuario";
                    return RedirectToAction("Index");
                }

                db.Usuarios.Remove(usuario);
                db.SaveChanges();

                TempData["Success"] = "Usuario eliminado exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el usuario: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Admin/Usuarios/ResetPassword/5
        public ActionResult ResetPassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuarios usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(usuario);
        }

        // POST: Admin/Usuarios/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, string NewPassword, string ConfirmPassword)
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ModelState.AddModelError("NewPassword", "La contraseña es requerida");
            }
            else if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Usuarios usuario = db.Usuarios.Find(id);
                    if (usuario == null)
                    {
                        return HttpNotFound();
                    }

                    // Generar nuevo salt y hash
                    string claveAes = ConfigurationManager.AppSettings["ClaveAES"];
                    string salt = Guid.NewGuid().ToString();
                    usuario.Salt = salt;
                    usuario.ClaveHash = AesEncryption.Encrypt(NewPassword, claveAes);
                    usuario.FechaUltimoCambioPassword = DateTime.Now;
                    usuario.IntentosFallidos = 0;
                    usuario.Bloqueado = false;

                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Success"] = "Contraseña restablecida exitosamente";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al restablecer la contraseña: " + ex.Message);
                }
            }

            Usuarios usuarioView = db.Usuarios.Find(id);
            return View(usuarioView);
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
