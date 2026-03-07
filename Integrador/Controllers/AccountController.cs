using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Integrador.Filters;
using Integrador.Helpers;
using Integrador.Models;
using Integrador.Models.ViewModels;

namespace Integrador.Controllers
{
    public class AccountController : Controller
    {
        private adopEntities db = new adopEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var usuario = db.Usuarios.FirstOrDefault(u => u.Email == model.Email && u.EstaActivo);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos");
                return View(model);
            }

            if (usuario.Bloqueado)
            {
                ModelState.AddModelError("", "Usuario bloqueado por intentos fallidos");
                return View(model);
            }

            string claveAes = ConfigurationManager.AppSettings["ClaveAES"];
            string passwordIngresada = AesEncryption.Encrypt(model.Password, claveAes);

            if (passwordIngresada != usuario.ClaveHash)
            {
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= 3)
                {
                    usuario.Bloqueado = true;
                    db.SaveChanges();

                    ModelState.AddModelError("", "Usuario bloqueado por 3 intentos fallidos.");
                    return View(model);
                }

                db.SaveChanges();

                ModelState.AddModelError("", "Contraseña incorrecta. Intentos restantes: " + (3 - usuario.IntentosFallidos));
                return View(model);
            }

            // Login correcto: autenticar directamente (NO 2FA en login)
            usuario.IntentosFallidos = 0;
            db.SaveChanges();

            // IMPORTANTE: Limpiar caché de permisos antiguos
            var returnUrl = Session["ReturnUrl"] as string;
            Session.Clear();

            Session["UsuarioId"] = usuario.Id;
            Session["Nombre"] = usuario.Nombres;
            Session["Rol"] = usuario.Rol;
            // NO establecer Session["PermisosUsuario"] aquí
            // Se cargará automáticamente en el primer request por CargarPermisosAttribute

            // Si hay una URL de retorno (ej: adoptar mascota), ir ahí
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Redirigir según el rol
            if (usuario.Rol == "Administrador")
                return RedirectToAction("Index", "Admin", new { area = "Admin" });

            // Para ciudadanos o cualquier otro rol, ir al área de Ciudadano
            return RedirectToAction("Index", "Ciudadano");
        }

        public ActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var files = model.Fotos ?? new HttpPostedFileBase[] { };
            bool anyFile = files.Any(f => f != null && f.ContentLength > 0);
            if (!anyFile)
            {
                ModelState.AddModelError("Fotos", "Seleccione al menos una imagen (JPG o PNG).");
                return View(model);
            }

            // Validación de imágenes: solo JPG/PNG, máximo 4MB, máximo 5 archivos
            var allowedExts = new[] { ".jpg", ".jpeg", ".png" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png" };
            const int maxBytes = 4 * 1024 * 1024; // 4 MB
            const int maxFiles = 5;

            int fileCount = files.Count(f => f != null && f.ContentLength > 0);
            if (fileCount > maxFiles)
            {
                ModelState.AddModelError("Fotos", $"Máximo {maxFiles} imágenes permitidas.");
                return View(model);
            }

            foreach (var file in files)
            {
                if (file == null || file.ContentLength == 0)
                    continue;

                var ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError("Fotos", $"El archivo '{file.FileName}' no es válido. Solo se permiten imágenes JPG o PNG.");
                    return View(model);
                }

                if (!allowedContentTypes.Contains(file.ContentType))
                {
                    ModelState.AddModelError("Fotos", $"El archivo '{file.FileName}' no es una imagen válida. Solo JPG o PNG.");
                    return View(model);
                }

                if (file.ContentLength > maxBytes)
                {
                    ModelState.AddModelError("Fotos", $"El archivo '{file.FileName}' excede el tamaño máximo de 4 MB.");
                    return View(model);
                }
            }

            if (db.Usuarios.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "El email ya está registrado");
                return View(model);
            }

            string claveAesRegistro = ConfigurationManager.AppSettings["ClaveAES"];
            string passwordCifrada = AesEncryption.Encrypt(model.Password, claveAesRegistro);
            string salt = Guid.NewGuid().ToString();

            Usuarios usuarioRegistro = new Usuarios
            {
                Nombres = model.Nombres,
                Apellidos = model.Apellidos,
                Email = model.Email,
                Cedula = model.Cedula,
                Telefono = model.Telefono,
                Direccion = model.Direccion,
                FechaNacimiento = model.FechaNacimiento,
                ClaveHash = passwordCifrada,
                Salt = salt,
                Rol = "Ciudadano",
                FechaRegistro = DateTime.Now,
                IntentosFallidos = 0,
                Bloqueado = false,
                EstaActivo = true
            };

            db.Usuarios.Add(usuarioRegistro);
            db.SaveChanges();

            // Guardar imágenes en tabla UsuarioImagenes
            try
            {
                var efConnString = ConfigurationManager.ConnectionStrings["adopEntities"].ConnectionString;
                var builder = new EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();

                    foreach (var file in files)
                    {
                        if (file == null || file.ContentLength == 0)
                            continue;

                        using (var br = new BinaryReader(file.InputStream))
                        {
                            var bytes = br.ReadBytes(file.ContentLength);

                            using (var cmd = new SqlCommand(
                                "INSERT INTO UsuarioImagenes (UsuarioId, NombreArchivo, ContentType, Data, FechaSubida) VALUES (@UsuarioId,@NombreArchivo,@ContentType,@Data,@FechaSubida); SELECT SCOPE_IDENTITY();",
                                conn))
                            {
                                cmd.Parameters.AddWithValue("@UsuarioId", usuarioRegistro.Id);
                                cmd.Parameters.AddWithValue("@NombreArchivo", Path.GetFileName(file.FileName));
                                cmd.Parameters.AddWithValue("@ContentType", file.ContentType);
                                var p = cmd.Parameters.Add("@Data", System.Data.SqlDbType.VarBinary, bytes.Length);
                                p.Value = bytes;
                                cmd.Parameters.AddWithValue("@FechaSubida", DateTime.Now);

                                var insertedIdObj = cmd.ExecuteScalar();
                                if (insertedIdObj != null && string.IsNullOrEmpty(usuarioRegistro.FotoPerfilRuta))
                                {
                                    var insertedId = Convert.ToInt32(insertedIdObj);
                                    usuarioRegistro.FotoPerfilRuta = Url.Action("ObtenerImagen", "Account", new { id = insertedId });
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["RegistroWarning"] = "Registro completado, pero falló al guardar imágenes: " + ex.Message;
            }

            // Crear notificación de bienvenida
            NotificacionHelper.NotificarBienvenida(db, usuarioRegistro.Id, usuarioRegistro.Nombres);

            TempData["RegistroSuccess"] = "¡Registro exitoso! Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        // --- RECUPERACIÓN: envía contraseña temporal + código por correo; Verify2FA valida el código; luego ResetPassword ---
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Ingrese un email válido.");
                return View();
            }

            var usuario = db.Usuarios.FirstOrDefault(u => u.Email == email && u.EstaActivo);
            if (usuario == null)
            {
                TempData["ForgotInfo"] = "Si el email existe en nuestro sistema, recibirás una contraseña temporal y un código de verificación por correo.";
                return RedirectToAction("Login");
            }

            string claveAes = ConfigurationManager.AppSettings["ClaveAES"] ?? "";
            string tempPasswordPlain = GenerateTemporaryPassword(8);
            string code = GenerateNumericCode(6);

            usuario.ClaveHash = AesEncryption.Encrypt(tempPasswordPlain, claveAes);
            usuario.TokenRecuperacion = code;
            usuario.TokenExpiracion = DateTime.Now.AddHours(24); // contraseña y código válidos 24 h
            db.SaveChanges();

            Session["RecoverUserId"] = usuario.Id;

            try
            {
                var from = ConfigurationManager.AppSettings["MailFrom"] ?? "no-reply@example.com";
                var subject = "Recuperación de contraseña - Pura Compañía";
                var verifyPath = Url.Action("Verify2FA", "Account");
                var verifyUrl = Request.Url != null
                    ? new Uri(Request.Url, verifyPath).ToString()
                    : (verifyPath ?? "/Account/Verify2FA");
                var sb = new StringBuilder();
                sb.AppendLine($"Hola {usuario.Nombres},");
                sb.AppendLine();
                sb.AppendLine("Has solicitado recuperar tu contraseña.");
                sb.AppendLine();
                sb.AppendLine("1) Tu contraseña temporal es:");
                sb.AppendLine($"   {tempPasswordPlain}");
                sb.AppendLine();
                sb.AppendLine("2) Tu código de verificación (ingrésalo en la siguiente pantalla) es:");
                sb.AppendLine($"   {code}");
                sb.AppendLine();
                sb.AppendLine("Ingresa el código en la pantalla de verificación para poder restablecer tu contraseña. La contraseña temporal y el código son válidos por 24 horas.");
                sb.AppendLine();
                sb.AppendLine("Si no solicitaste esto, ignora el mensaje.");
                sb.AppendLine();
                sb.AppendLine("Pantalla de verificación: " + verifyUrl);
                var body = sb.ToString();

                using (var msg = new MailMessage(from, usuario.Email, subject, body))
                using (var smtp = CrearSmtpClient())
                {
                    smtp.Send(msg);
                }

                TempData["VerifyInfo"] = "Revisa tu correo: te enviamos una contraseña temporal y un código. Ingresa el código a continuación para continuar.";
            }
            catch (Exception ex)
            {
                TempData["ForgotWarning"] = "Error al enviar el correo. Contacta con soporte. (" + ex.Message + ")";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Verify2FA");
        }

        // Página para introducir el código 2FA (usada ahora para recuperación)
        public ActionResult Verify2FA()
        {
            // Si no hay proceso pendiente de recuperación, redirige a login
            if (Session["RecoverUserId"] == null)
                return RedirectToAction("Login");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Verify2FA(string code, bool resend = false)
        {
            // Si no hay proceso de recuperación pendiente, volver a login
            if (Session["RecoverUserId"] == null)
                return RedirectToAction("Login");

            int userId = Convert.ToInt32(Session["RecoverUserId"]);
            var usuario = db.Usuarios.Find(userId);
            if (usuario == null)
            {
                Session.Remove("RecoverUserId");
                return RedirectToAction("Login");
            }

            if (resend)
            {
                // Regenerar código y reenviar por correo
                string newCode = GenerateNumericCode(6);
                usuario.TokenRecuperacion = newCode;
                usuario.TokenExpiracion = DateTime.Now.AddHours(24); // mismo margen que en ForgotPassword
                db.SaveChanges();

                try
                {
                    var from = ConfigurationManager.AppSettings["MailFrom"] ?? "no-reply@example.com";
                    var subject = "Nuevo código de verificación - Pura Compañía";
                    var body = $"Hola {usuario.Nombres},\n\nTu nuevo código de verificación es: {newCode}\n\nIngrésalo en la pantalla de verificación para continuar al restablecimiento de contraseña. El código es válido por 24 horas.\n\nSi no solicitaste el reenvío, ignora este mensaje.";
                    using (var msg = new MailMessage(from, usuario.Email, subject, body))
                    using (var smtp = CrearSmtpClient())
                    {
                        smtp.Send(msg);
                    }
                    TempData["VerifyInfo"] = "Se ha reenviado el código a tu correo.";
                }
                catch
                {
                    TempData["VerifyWarning"] = "No ha sido posible reenviar el código. Intenta nuevamente más tarde.";
                }

                return RedirectToAction("Verify2FA");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "Ingresa el código de verificación.");
                return View();
            }

            // Verificar expiración
            if (!usuario.TokenExpiracion.HasValue || usuario.TokenExpiracion.Value < DateTime.Now)
            {
                ModelState.AddModelError("", "El código ha expirado. Solicita un reenvío.");
                return View();
            }

            if (usuario.TokenRecuperacion != code)
            {
                ModelState.AddModelError("", "Código inválido.");
                return View();
            }

            // Código verificado: permitir cambiar contraseña
            // Marcar verificación en sesión y redirigir a pantalla de cambio de contraseña
            Session["RecoverVerifiedUserId"] = usuario.Id;

            // Limpiar token de verificación (opcional)
            usuario.TokenRecuperacion = null;
            usuario.TokenExpiracion = null;
            db.SaveChanges();

            return RedirectToAction("ResetPassword");
        }

        public ActionResult ResetPassword()
        {
            // Si venimos por flujo de recuperación y ya verificaron el código
            if (Session["RecoverVerifiedUserId"] != null)
            {
                // Mostrar formulario para nueva contraseña (sin pedir contraseña temporal)
                ViewBag.Recovery = true;
                return View();
            }

            // flujo previo (si se mantiene): mostrar vista estándar
            ViewBag.Recovery = false;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string email, string temporaryPassword, string newPassword, string confirmPassword)
        {
            // Si estamos en flujo de recuperación verificado por código
            if (Session["RecoverVerifiedUserId"] != null)
            {
                if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ModelState.AddModelError("", "Todos los campos son obligatorios.");
                    ViewBag.Recovery = true;
                    return View();
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Las contraseñas nuevas no coinciden.");
                    ViewBag.Recovery = true;
                    return View();
                }

                if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("", "La nueva contraseña debe tener al menos 6 caracteres.");
                    ViewBag.Recovery = true;
                    return View();
                }

                int userId = Convert.ToInt32(Session["RecoverVerifiedUserId"]);
                var usuario = db.Usuarios.Find(userId);
                if (usuario == null)
                {
                    Session.Remove("RecoverVerifiedUserId");
                    return RedirectToAction("Login");
                }

                string claveAes = ConfigurationManager.AppSettings["ClaveAES"] ?? "";
                usuario.ClaveHash = AesEncryption.Encrypt(newPassword, claveAes);
                usuario.TokenExpiracion = null;
                usuario.TokenRecuperacion = null;
                usuario.IntentosFallidos = 0;
                usuario.Bloqueado = false;
                db.SaveChanges();

                Session.Remove("RecoverVerifiedUserId");
                Session.Remove("RecoverUserId");

                TempData["ResetSuccess"] = "Contraseña actualizada correctamente. Puedes iniciar sesión.";
                return RedirectToAction("Login");
            }

            // Si no es flujo de recuperación por código, mantener el flujo previo (temporal)
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(temporaryPassword) ||
                string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas nuevas no coinciden.");
                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "La nueva contraseña debe tener al menos 6 caracteres.");
                return View();
            }

            var usuarioOld = db.Usuarios.FirstOrDefault(u => u.Email == email && u.EstaActivo);
            if (usuarioOld == null)
            {
                ModelState.AddModelError("", "Email o contraseña temporal inválidos.");
                return View();
            }

            if (!usuarioOld.TokenExpiracion.HasValue || usuarioOld.TokenExpiracion.Value < DateTime.Now)
            {
                ModelState.AddModelError("", "La contraseña temporal ya expiró. Solicita una nueva recuperación.");
                return View();
            }

            string claveAesOld = ConfigurationManager.AppSettings["ClaveAES"] ?? "";
            string tempHash = AesEncryption.Encrypt(temporaryPassword, claveAesOld);

            if (tempHash != usuarioOld.ClaveHash)
            {
                ModelState.AddModelError("", "Contraseña temporal inválida.");
                return View();
            }

            usuarioOld.ClaveHash = AesEncryption.Encrypt(newPassword, claveAesOld);
            usuarioOld.TokenExpiracion = null;
            usuarioOld.TokenRecuperacion = null;
            usuarioOld.IntentosFallidos = 0;
            usuarioOld.Bloqueado = false;
            db.SaveChanges();

            TempData["ResetSuccess"] = "Contraseña actualizada correctamente. Puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Helpers
        /// <summary>
        /// Crea un SmtpClient configurado desde Web.config (system.net/mailSettings o appSettings).
        /// Permite envío de correo por SMTP para recuperación de contraseña.
        /// </summary>
        private static SmtpClient CrearSmtpClient()
        {
            var smtp = new SmtpClient();
            var host = ConfigurationManager.AppSettings["SmtpHost"];
            var portStr = ConfigurationManager.AppSettings["SmtpPort"];
            var user = ConfigurationManager.AppSettings["SmtpUser"];
            var pass = ConfigurationManager.AppSettings["SmtpPass"];
            var enableSslStr = ConfigurationManager.AppSettings["SmtpEnableSsl"];

            if (!string.IsNullOrWhiteSpace(host))
            {
                smtp.Host = host;
                if (!string.IsNullOrWhiteSpace(portStr) && int.TryParse(portStr, out int port))
                    smtp.Port = port;
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                    smtp.Credentials = new System.Net.NetworkCredential(user, pass);
                if (!string.IsNullOrWhiteSpace(enableSslStr) && bool.TryParse(enableSslStr, out bool enableSsl))
                    smtp.EnableSsl = enableSsl;
            }
            return smtp;
        }

        private string GenerateNumericCode(int digits = 6)
        {
            var rnd = new Random();
            var min = (int)Math.Pow(10, digits - 1);
            var max = (int)Math.Pow(10, digits) - 1;
            return rnd.Next(min, max).ToString();
        }

        private string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var sb = new StringBuilder();
            var rnd = new Random();
            for (int i = 0; i < length; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);
            return sb.ToString();
        }

        // =====================================================================
        // GESTIÓN DE IMÁGENES DEL USUARIO
        // =====================================================================

        /// <summary>
        /// Muestra la galería de imágenes del usuario actual
        /// </summary>
        [CargarPermisos]
        public ActionResult Imagenes()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login");

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);

            // Obtener todas las imágenes del usuario desde la base de datos
            var imagenes = new System.Collections.Generic.List<UsuarioImagen>();

            try
            {
                var efConnString = ConfigurationManager.ConnectionStrings["adopEntities"].ConnectionString;
                var builder = new EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Id, UsuarioId, NombreArchivo, ContentType, FechaSubida FROM UsuarioImagenes WHERE UsuarioId = @UsuarioId ORDER BY FechaSubida DESC",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                imagenes.Add(new UsuarioImagen
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                                    NombreArchivo = reader["NombreArchivo"] as string,
                                    ContentType = reader["ContentType"] as string,
                                    FechaSubida = Convert.ToDateTime(reader["FechaSubida"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ImagenesError"] = "Error al cargar las imágenes: " + ex.Message;
            }

            return View(imagenes);
        }

        /// <summary>
        /// Obtiene una imagen específica de la base de datos por su ID
        /// </summary>
        public ActionResult ObtenerImagen(int id)
        {
            try
            {
                var efConnString = ConfigurationManager.ConnectionStrings["adopEntities"].ConnectionString;
                var builder = new EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Data, ContentType FROM UsuarioImagenes WHERE Id = @Id",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var data = reader["Data"] as byte[];
                                var contentType = reader["ContentType"] as string;

                                if (data != null && !string.IsNullOrEmpty(contentType))
                                {
                                    return File(data, contentType);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si hay error, devolver imagen por defecto
            }

            // Devolver imagen por defecto si no se encuentra
            var defaultPath = Server.MapPath("~/Content/images/default-avatar.png");
            if (System.IO.File.Exists(defaultPath))
            {
                return File(defaultPath, "image/png");
            }

            return HttpNotFound();
        }

        /// <summary>
        /// Establece una imagen como foto de perfil del usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EstablecerPerfil(int id)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login");

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);

            try
            {
                // Verificar que la imagen pertenece al usuario
                var efConnString = ConfigurationManager.ConnectionStrings["adopEntities"].ConnectionString;
                var builder = new EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                bool esDelUsuario = false;
                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM UsuarioImagenes WHERE Id = @Id AND UsuarioId = @UsuarioId",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        esDelUsuario = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }

                if (!esDelUsuario)
                {
                    TempData["ImagenesError"] = "No tienes permiso para establecer esta imagen.";
                    return RedirectToAction("Imagenes");
                }

                // Actualizar la foto de perfil del usuario
                var usuario = db.Usuarios.Find(usuarioId);
                if (usuario != null)
                {
                    usuario.FotoPerfilRuta = Url.Action("ObtenerImagen", "Account", new { id = id });
                    db.SaveChanges();
                    TempData["ImagenesSuccess"] = "Foto de perfil actualizada correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ImagenesError"] = "Error al establecer la foto de perfil: " + ex.Message;
            }

            return RedirectToAction("Imagenes");
        }

        /// <summary>
        /// Elimina una imagen del usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarImagen(int id)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login");

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);

            try
            {
                var efConnString = ConfigurationManager.ConnectionStrings["adopEntities"].ConnectionString;
                var builder = new EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();

                    // Verificar que la imagen pertenece al usuario
                    using (var cmdCheck = new SqlCommand(
                        "SELECT COUNT(*) FROM UsuarioImagenes WHERE Id = @Id AND UsuarioId = @UsuarioId",
                        conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@Id", id);
                        cmdCheck.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        if (Convert.ToInt32(cmdCheck.ExecuteScalar()) == 0)
                        {
                            TempData["ImagenesError"] = "No tienes permiso para eliminar esta imagen.";
                            return RedirectToAction("Imagenes");
                        }
                    }

                    // Eliminar la imagen
                    using (var cmdDelete = new SqlCommand(
                        "DELETE FROM UsuarioImagenes WHERE Id = @Id",
                        conn))
                    {
                        cmdDelete.Parameters.AddWithValue("@Id", id);
                        cmdDelete.ExecuteNonQuery();
                    }

                    // Si era la foto de perfil, limpiarla
                    var usuario = db.Usuarios.Find(usuarioId);
                    if (usuario != null && !string.IsNullOrEmpty(usuario.FotoPerfilRuta))
                    {
                        var imagenUrl = Url.Action("ObtenerImagen", "Account", new { id = id });
                        if (usuario.FotoPerfilRuta.Contains($"id={id}"))
                        {
                            usuario.FotoPerfilRuta = null;
                            db.SaveChanges();
                        }
                    }

                    TempData["ImagenesSuccess"] = "Imagen eliminada correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ImagenesError"] = "Error al eliminar la imagen: " + ex.Message;
            }

            return RedirectToAction("Imagenes");
        }
    }
}