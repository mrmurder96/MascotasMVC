using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models.ViewModels;
using Integrador.Models;

namespace Integrador.Controllers
{
    public class AdopcionesController : Controller
    {
        private string GetProviderConnectionString()
        {
            var connSetting = ConfigurationManager.ConnectionStrings["AdopcionMascotasEntities"];
            if (connSetting == null)
                throw new InvalidOperationException("ConnectionString 'AdopcionMascotasEntities' no encontrada en web.config.");

            var efConnString = connSetting.ConnectionString;
            var builder = new EntityConnectionStringBuilder(efConnString);
            return builder.ProviderConnectionString;
        }

        // GET: /Adopciones/Create?mascotaId=123&nombreMascota=Fiesta
        [Authorize]
        public ActionResult Create(int mascotaId, string nombreMascota = null)
        {
            var model = new SolicitudAdopcionViewModel
            {
                MascotaId = mascotaId,
                MascotaNombre = nombreMascota,
                FechaSolicitud = DateTime.Now,
                Nombre = Session["Nombre"] as string ?? "",
                Email = Session["Email"] as string ?? ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(SolicitudAdopcionViewModel model)
        {

            if (!ModelState.IsValid)
            {
                TempData["AdopcionError"] = "Corrige los datos del formulario.";
                return View(model);
            }

            model.FechaSolicitud = DateTime.Now;
            model.Estado = "Pendiente";

            int usuarioId = Session["UsuarioId"] != null ? Convert.ToInt32(Session["UsuarioId"]) : 0;
            if (usuarioId == 0)
            {
                TempData["AdopcionError"] = "Debes iniciar sesión para enviar una solicitud.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var connStr = GetProviderConnectionString();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(@"INSERT INTO SolicitudesAdopcion
                    (UsuarioId, MascotaId, MascotaNombre, FechaSolicitud, Estado, Nombre, Apellidos, Email, Telefono, Direccion, FechaPreferida, Motivo, Experiencia, Referencias, Observaciones)
                    VALUES (@UsuarioId,@MascotaId,@MascotaNombre,@FechaSolicitud,@Estado,@Nombre,@Apellidos,@Email,@Telefono,@Direccion,@FechaPreferida,@Motivo,@Experiencia,@Referencias,@Observaciones);
                    SELECT SCOPE_IDENTITY();", conn))
                {
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmd.Parameters.AddWithValue("@MascotaId", model.MascotaId);
                    cmd.Parameters.AddWithValue("@MascotaNombre", (object)model.MascotaNombre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaSolicitud", model.FechaSolicitud);
                    cmd.Parameters.AddWithValue("@Estado", model.Estado);
                    cmd.Parameters.AddWithValue("@Nombre", (object)model.Nombre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Apellidos", (object)model.Apellidos ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)model.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", (object)model.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", (object)model.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaPreferida", model.FechaPreferida.HasValue ? (object)model.FechaPreferida.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Motivo", (object)model.Motivo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Experiencia", (object)model.Experiencia ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Referencias", (object)model.Referencias ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Observaciones", (object)model.Observaciones ?? DBNull.Value);

                    conn.Open();
                    var idObj = cmd.ExecuteScalar();
                    if (idObj != null && int.TryParse(idObj.ToString(), out int newId))
                    {
                        model.Id = newId;
                        TempData["AdopcionSuccess"] = "Solicitud creada correctamente.";
                        return RedirectToAction("Index", "Ciudadano");
                    }
                }
            }
            catch (Exception ex)
            {
                // Guardado fallback en sesión si falla la BD (útil para demo)
                var list = Session["SolicitudesAdopcionTemp"] as List<SolicitudAdopcionViewModel> ?? new List<SolicitudAdopcionViewModel>();
                model.Id = list.Count > 0 ? list.Max(x => x.Id) + 1 : 1;
                list.Add(model);
                Session["SolicitudesAdopcionTemp"] = list;
                TempData["AdopcionWarning"] = "No fue posible guardar en BD; la solicitud se guardó temporalmente para la demostración.";
                return RedirectToAction("Index", "Ciudadano");
            }

            TempData["AdopcionError"] = "No fue posible crear la solicitud. Intenta de nuevo.";
            return View(model);
        }

        // Devuelve partial con las solicitudes del usuario (puede llamarse desde el perfil)
        public ActionResult MySolicitudes()
        {
            int usuarioId = Session["UsuarioId"] != null ? Convert.ToInt32(Session["UsuarioId"]) : 0;
            var lista = new List<SolicitudAdopcionViewModel>();

            if (usuarioId == 0)
                return PartialView("_MisSolicitudes", lista);

            try
            {
                var connStr = GetProviderConnectionString();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand(
                    @"SELECT Id, MascotaId, MascotaNombre, FechaSolicitud, Estado, Nombre, Apellidos, Email, Telefono, Direccion, FechaPreferida, Motivo, Experiencia, Referencias, Observaciones
                      FROM SolicitudesAdopcion WHERE UsuarioId = @UsuarioId ORDER BY FechaSolicitud DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            lista.Add(new SolicitudAdopcionViewModel
                            {
                                Id = !rdr.IsDBNull(0) ? rdr.GetInt32(0) : 0,
                                MascotaId = !rdr.IsDBNull(1) ? rdr.GetInt32(1) : 0,
                                MascotaNombre = !rdr.IsDBNull(2) ? rdr.GetString(2) : null,
                                FechaSolicitud = !rdr.IsDBNull(3) ? rdr.GetDateTime(3) : DateTime.MinValue,
                                Estado = !rdr.IsDBNull(4) ? rdr.GetString(4) : "Pendiente",
                                Nombre = !rdr.IsDBNull(5) ? rdr.GetString(5) : null,
                                Apellidos = !rdr.IsDBNull(6) ? rdr.GetString(6) : null,
                                Email = !rdr.IsDBNull(7) ? rdr.GetString(7) : null,
                                Telefono = !rdr.IsDBNull(8) ? rdr.GetString(8) : null,
                                Direccion = !rdr.IsDBNull(9) ? rdr.GetString(9) : null,
                                FechaPreferida = !rdr.IsDBNull(10) ? (DateTime?)rdr.GetDateTime(10) : null,
                                Motivo = !rdr.IsDBNull(11) ? rdr.GetString(11) : null,
                                Experiencia = !rdr.IsDBNull(12) ? rdr.GetString(12) : null,
                                Referencias = !rdr.IsDBNull(13) ? rdr.GetString(13) : null,
                                Observaciones = !rdr.IsDBNull(14) ? rdr.GetString(14) : null
                            });
                        }
                    }
                }
            }
            catch
            {
                // fallback: leer desde sesión temporal
                var temp = Session["SolicitudesAdopcionTemp"] as List<SolicitudAdopcionViewModel>;
                if (temp != null)
                    lista.AddRange(temp.OrderByDescending(s => s.FechaSolicitud));
            }

            return PartialView("_MisSolicitudes", lista);
        }
    }
}