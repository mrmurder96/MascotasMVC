using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;

namespace Integrador.Controllers
{
    public class CampanasController : Controller
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

        // Index: el controlador determina si el usuario está autenticado y pasa ese estado a la vista.
        public ActionResult Index()
        {
            bool isAuth = User?.Identity?.IsAuthenticated ?? false;
            ViewBag.IsAuthenticated = isAuth;

            if (!isAuth)
            {
                // Usuario no autenticado: no cargamos campañas
                return View(new List<Campania>());
            }

            // Usuario autenticado: cargar campañas
            var lista = GetCampanas();
            return View(lista);
        }

        [Authorize]
        public ActionResult MisCampanas()
        {
            var lista = GetCampanas();
            return View(lista);
        }

        public ActionResult Details(int id)
        {
            Campania c = null;
            try
            {
                var connStr = GetProviderConnectionString();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT Id, Titulo, Descripcion, FechaInicio, FechaFin, Activa, ImagenUrl FROM Campanias WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            var ordId = rdr.GetOrdinal("Id");
                            var ordTitulo = rdr.GetOrdinal("Titulo");
                            var ordDescripcion = rdr.GetOrdinal("Descripcion");
                            var ordFechaInicio = rdr.GetOrdinal("FechaInicio");
                            var ordFechaFin = rdr.GetOrdinal("FechaFin");
                            var ordActiva = rdr.GetOrdinal("Activa");
                            var ordImagenUrl = rdr.GetOrdinal("ImagenUrl");

                            c = new Campania
                            {
                                Id = rdr.IsDBNull(ordId) ? 0 : rdr.GetInt32(ordId),
                                Titulo = rdr.IsDBNull(ordTitulo) ? null : rdr.GetString(ordTitulo),
                                Descripcion = rdr.IsDBNull(ordDescripcion) ? null : rdr.GetString(ordDescripcion),
                                FechaInicio = rdr.IsDBNull(ordFechaInicio) ? DateTime.MinValue : rdr.GetDateTime(ordFechaInicio),
                                FechaFin = rdr.IsDBNull(ordFechaFin) ? (DateTime?)null : rdr.GetDateTime(ordFechaFin),
                                Activa = rdr.IsDBNull(ordActiva) ? false : Convert.ToBoolean(rdr.GetValue(ordActiva)),
                                ImagenUrl = rdr.IsDBNull(ordImagenUrl) ? null : rdr.GetString(ordImagenUrl)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["CampanasError"] = "Error cargando la campaña: " + ex.Message;
            }

            if (c == null)
                return HttpNotFound();

            return View(c);
        }

        private List<Campania> GetCampanas()
        {
            var lista = new List<Campania>();
            try
            {
                var connStr = GetProviderConnectionString();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT Id, Titulo, Descripcion, FechaInicio, FechaFin, Activa, ImagenUrl FROM Campanias WHERE Activa = 1 ORDER BY FechaInicio DESC", conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var ordId = rdr.GetOrdinal("Id");
                            var ordTitulo = rdr.GetOrdinal("Titulo");
                            var ordDescripcion = rdr.GetOrdinal("Descripcion");
                            var ordFechaInicio = rdr.GetOrdinal("FechaInicio");
                            var ordFechaFin = rdr.GetOrdinal("FechaFin");
                            var ordActiva = rdr.GetOrdinal("Activa");
                            var ordImagenUrl = rdr.GetOrdinal("ImagenUrl");

                            var camp = new Campania
                            {
                                Id = rdr.IsDBNull(ordId) ? 0 : rdr.GetInt32(ordId),
                                Titulo = rdr.IsDBNull(ordTitulo) ? null : rdr.GetString(ordTitulo),
                                Descripcion = rdr.IsDBNull(ordDescripcion) ? null : rdr.GetString(ordDescripcion),
                                FechaInicio = rdr.IsDBNull(ordFechaInicio) ? DateTime.MinValue : rdr.GetDateTime(ordFechaInicio),
                                FechaFin = rdr.IsDBNull(ordFechaFin) ? (DateTime?)null : rdr.GetDateTime(ordFechaFin),
                                Activa = rdr.IsDBNull(ordActiva) ? false : Convert.ToBoolean(rdr.GetValue(ordActiva)),
                                ImagenUrl = rdr.IsDBNull(ordImagenUrl) ? null : rdr.GetString(ordImagenUrl)
                            };

                            lista.Add(camp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["CampanasError"] = "No se pudieron cargar las campañas: " + ex.Message;
            }

            return lista;
        }

        // Añade al final de la clase AccountController (temporal, para diagnóstico)
        public ActionResult DebugSession()
        {
            return Json(new {
                SessionUsuarioId = Session["UsuarioId"],
                SessionNombre = Session["Nombre"],
                SessionRol = Session["Rol"],
                IsAuthenticated = User?.Identity?.IsAuthenticated ?? false
            }, JsonRequestBehavior.AllowGet);
        }
    }
}