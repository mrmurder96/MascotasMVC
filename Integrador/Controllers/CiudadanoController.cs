using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Models.ViewModels;

namespace Integrador.Controllers
{
    public class CiudadanoController : Controller
    {
        private readonly AdopcionMascotasEntities db = new AdopcionMascotasEntities();

        public ActionResult Index()
        {
            return View();
        }

        // GET: /Ciudadano/Perfil
        public ActionResult Perfil()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario == null)
                return HttpNotFound();

            var model = new PerfilViewModel
            {
                Id = usuario.Id,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                FotoPerfilRuta = string.IsNullOrEmpty(usuario.FotoPerfilRuta) ? "/Content/images/default-avatar.png" : usuario.FotoPerfilRuta
            };

            return View(model);
        }

        // POST: /Ciudadano/Perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Perfil(PerfilViewModel model)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);
            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario == null)
                return HttpNotFound();

            // Validar email si cambia
            if (!string.Equals(usuario.Email, model.Email, StringComparison.OrdinalIgnoreCase)
                && db.Usuarios.Any(u => u.Email == model.Email && u.Id != usuarioId))
            {
                ModelState.AddModelError("Email", "El email ya está registrado por otro usuario.");
                return View(model);
            }

            // Procesar subida de nuevas imágenes (si las hay)
            var files = model.NuevasFotos ?? new System.Web.HttpPostedFileBase[] { };
            var anyFile = files.Any(f => f != null && f.ContentLength > 0);

            if (anyFile)
            {
                var allowedExts = new[] { ".jpg", ".jpeg", ".png" };
                var allowedContentTypes = new[] { "image/jpeg", "image/png" };
                const int maxBytes = 2 * 1024 * 1024; // 2 MB

                foreach (var file in files)
                {
                    if (file == null || file.ContentLength == 0) continue;

                    var ext = System.IO.Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
                    if (!allowedExts.Contains(ext) || !allowedContentTypes.Contains(file.ContentType) || file.ContentLength > maxBytes)
                    {
                        ModelState.AddModelError("NuevasFotos", "Solo imágenes JPG/PNG y ≤ 2MB.");
                        return View(model);
                    }
                }

                try
                {
                    var efConnString = ConfigurationManager.ConnectionStrings["AdopcionMascotasEntities"].ConnectionString;
                    var builder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(efConnString);
                    var providerConnStr = builder.ProviderConnectionString;

                    using (var conn = new SqlConnection(providerConnStr))
                    {
                        conn.Open();
                        foreach (var file in files)
                        {
                            if (file == null || file.ContentLength == 0) continue;

                            using (var br = new System.IO.BinaryReader(file.InputStream))
                            {
                                var bytes = br.ReadBytes(file.ContentLength);

                                using (var cmd = new SqlCommand(
                                    "INSERT INTO UsuarioImagenes (UsuarioId, NombreArchivo, ContentType, Data, FechaSubida) VALUES (@UsuarioId,@NombreArchivo,@ContentType,@Data,@FechaSubida); SELECT SCOPE_IDENTITY();",
                                    conn))
                                {
                                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                                    cmd.Parameters.AddWithValue("@NombreArchivo", System.IO.Path.GetFileName(file.FileName));
                                    cmd.Parameters.AddWithValue("@ContentType", file.ContentType);
                                    var p = cmd.Parameters.Add("@Data", System.Data.SqlDbType.VarBinary, bytes.Length);
                                    p.Value = bytes;
                                    cmd.Parameters.AddWithValue("@FechaSubida", DateTime.Now);

                                    var insertedIdObj = cmd.ExecuteScalar();
                                    if (insertedIdObj != null && string.IsNullOrEmpty(usuario.FotoPerfilRuta))
                                    {
                                        var insertedId = Convert.ToInt32(insertedIdObj);
                                        usuario.FotoPerfilRuta = Url.Action("ObtenerImagen", "Account", new { id = insertedId });
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "No se pudieron guardar las imágenes: " + ex.Message);
                    return View(model);
                }
            }

            // Actualizar datos del usuario
            usuario.Nombres = model.Nombres;
            usuario.Apellidos = model.Apellidos;
            usuario.Email = model.Email;
            usuario.Telefono = model.Telefono;
            usuario.Direccion = model.Direccion;

            db.SaveChanges();

            TempData["PerfilSuccess"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }

        // GET: /Ciudadano/Centros
        public ActionResult Centros()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            var lista = new List<Centro>();
            try
            {
                var efConnString = ConfigurationManager.ConnectionStrings["AdopcionMascotasEntities"].ConnectionString;
                var builder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT Id, Nombre, Direccion, Lat, Lng, Telefono FROM Centros ORDER BY Id", conn))
                    {
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                lista.Add(new Centro
                                {
                                    Id = Convert.ToInt32(rdr["Id"]),
                                    Nombre = rdr["Nombre"] as string,
                                    Direccion = rdr["Direccion"] as string,
                                    Lat = Convert.ToDouble(rdr["Lat"]),
                                    Lng = Convert.ToDouble(rdr["Lng"]),
                                    Telefono = rdr["Telefono"] as string
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["CentrosError"] = "No se pudieron cargar los centros: " + ex.Message;
            }

            // Pasar API key a la vista (configurar en web.config appSettings)
            ViewBag.GoogleMapsApiKey = ConfigurationManager.AppSettings["GoogleMapsApiKey"] ?? "";

            return View(lista);
        }

        // POST: /Ciudadano/SeleccionarCentro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SeleccionarCentro(int id)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);

            // comprobar que el centro exista
            bool existe = false;
            try
            {
                var efConnString = ConfigurationManager.ConnectionStrings["AdopcionMascotasEntities"].ConnectionString;
                var builder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new SqlConnection(providerConnStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Centros WHERE Id = @Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        existe = count > 0;
                    }
                }
            }
            catch
            {
                existe = false;
            }

            if (!existe)
                return HttpNotFound();

            var usuario = db.Usuarios.Find(usuarioId);
            if (usuario != null)
            {
                // Guardamos el id del centro en TokenRecuperacion como campo temporal para evitar tocar EDMX.
                usuario.TokenRecuperacion = id.ToString();
                db.SaveChanges();
                TempData["CentroSelected"] = "Centro seleccionado correctamente.";
            }

            return RedirectToAction("Centros");
        }

        // GET: /Ciudadano/Notificaciones
        public ActionResult Notificaciones()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            int usuarioId = Convert.ToInt32(Session["UsuarioId"]);
            var lista = new List<Notificacion>();

            try
            {
                var connSetting = System.Configuration.ConfigurationManager.ConnectionStrings["AdopcionMascotasEntities"];
                if (connSetting == null)
                {
                    TempData["NotificacionesError"] = "ConnectionString 'AdopcionMascotasEntities' no encontrada en web.config.";
                    return View(lista);
                }

                var efConnString = connSetting.ConnectionString;
                var builder = new System.Data.Entity.Core.EntityClient.EntityConnectionStringBuilder(efConnString);
                var providerConnStr = builder.ProviderConnectionString;

                using (var conn = new System.Data.SqlClient.SqlConnection(providerConnStr))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT Id, UsuarioId, Titulo, Mensaje, Leido, Fecha FROM Notificaciones WHERE UsuarioId = @UsuarioId ORDER BY Fecha DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                lista.Add(new Notificacion
                                {
                                    Id = Convert.ToInt32(rdr["Id"]),
                                    UsuarioId = Convert.ToInt32(rdr["UsuarioId"]),
                                    Titulo = rdr["Titulo"] as string,
                                    Mensaje = rdr["Mensaje"] as string,
                                    Leido = Convert.ToBoolean(rdr["Leido"]),
                                    Fecha = Convert.ToDateTime(rdr["Fecha"])
                                });
                            }
                        }
                    }
                }

                if (!lista.Any())
                    TempData["NotificacionesInfo"] = "No tienes notificaciones.";
            }
            catch (Exception ex)
            {
                // Mensaje directo para depuración mínima; después puedes loggear en un sistema real
                TempData["NotificacionesError"] = "No se pudieron cargar las notificaciones: " + ex.Message;
            }

            return View(lista);
        }
    }
}
