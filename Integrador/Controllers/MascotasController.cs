using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Web.Mvc;
using Integrador.Models;
using Integrador.Filters;

namespace Integrador.Controllers
{
    [CargarPermisos]
    public class MascotasController : Controller
    {
        private string GetProviderConnectionString()
        {
            var efConnString = ConfigurationManager.ConnectionStrings["AdopcionMascotasEntities"].ConnectionString;
            var builder = new EntityConnectionStringBuilder(efConnString);
            return builder.ProviderConnectionString;
        }

        public ActionResult Index()
        {
            var lista = new List<Mascota>();
            try
            {
                var connStr = GetProviderConnectionString();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT Id, Nombre, Tipo, Edad, Ubicacion, Descripcion, FotoUrl, Estado FROM Mascotas WHERE Estado = 'Disponible' ORDER BY Nombre", conn))
                {
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            lista.Add(new Mascota
                            {
                                Id = (int)rdr["Id"],
                                Nombre = rdr["Nombre"] as string,
                                Tipo = rdr["Tipo"] as string,
                                Edad = rdr["Edad"] as int? ?? 0,
                                Ubicacion = rdr["Ubicacion"] as string,
                                Descripcion = rdr["Descripcion"] as string,
                                FotoUrl = rdr["FotoUrl"] as string,
                                Estado = rdr["Estado"] as string
                            });
                        }
                    }
                }
            }
            catch
            {
                // ignorar errores por rapidez; en producción loguear
            }
            return View(lista);
        }

        public ActionResult Details(int id)
        {
            Mascota m = null;
            try
            {
                var connStr = GetProviderConnectionString();
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("SELECT Id, Nombre, Tipo, Edad, Ubicacion, Descripcion, FotoUrl, Estado FROM Mascotas WHERE Id = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            m = new Mascota
                            {
                                Id = (int)rdr["Id"],
                                Nombre = rdr["Nombre"] as string,
                                Tipo = rdr["Tipo"] as string,
                                Edad = rdr["Edad"] as int? ?? 0,
                                Ubicacion = rdr["Ubicacion"] as string,
                                Descripcion = rdr["Descripcion"] as string,
                                FotoUrl = rdr["FotoUrl"] as string,
                                Estado = rdr["Estado"] as string
                            };
                        }
                    }
                }
            }
            catch
            {
            }

            if (m == null)
                return HttpNotFound();

            return View(m);
        }

        [HttpGet]
        public ActionResult Adopt(int id)
        {
            // Requiere sesión para iniciar adopción
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            // Cargar mascota para mostrar en el formulario
            var mascota = new Mascota();
            var connStr = GetProviderConnectionString();
            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand("SELECT Id, Nombre, Tipo, Edad, Ubicacion, Descripcion, FotoUrl, Estado FROM Mascotas WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        mascota = new Mascota
                        {
                            Id = (int)rdr["Id"],
                            Nombre = rdr["Nombre"] as string,
                            Tipo = rdr["Tipo"] as string,
                            Edad = rdr["Edad"] as int? ?? 0,
                            Ubicacion = rdr["Ubicacion"] as string,
                            Descripcion = rdr["Descripcion"] as string,
                            FotoUrl = rdr["FotoUrl"] as string,
                            Estado = rdr["Estado"] as string
                        };
                    }
                }
            }

            if (mascota == null || mascota.Id == 0)
                return HttpNotFound();

            return View(mascota);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adopt(int id, string nombreSolicitante, string email, string telefono)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(nombreSolicitante))
            {
                ModelState.AddModelError("", "Nombre y email son obligatorios.");
                var m = new Mascota { Id = id };
                return View(m);
            }

            var usuarioId = Convert.ToInt32(Session["UsuarioId"]);
            var connStr = GetProviderConnectionString();

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        // Insert solicitud de adopción
                        using (var cmd = new SqlCommand("INSERT INTO Adopciones (MascotaId, UsuarioId, NombreSolicitante, Email, Telefono, FechaSolicitud, Estado) VALUES (@MascotaId,@UsuarioId,@NombreSolicitante,@Email,@Telefono,GETDATE(),'En proceso'); SELECT SCOPE_IDENTITY();", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@MascotaId", id);
                            cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                            cmd.Parameters.AddWithValue("@NombreSolicitante", nombreSolicitante);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@Telefono", telefono ?? (object)DBNull.Value);

                            var inserted = cmd.ExecuteScalar();
                        }

                        // Actualizar estado de mascota
                        using (var cmd2 = new SqlCommand("UPDATE Mascotas SET Estado = 'En proceso' WHERE Id = @Id", conn, tran))
                        {
                            cmd2.Parameters.AddWithValue("@Id", id);
                            cmd2.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                }

                TempData["AdoptInfo"] = "Solicitud enviada. La mascota queda en estado 'En proceso de adopción'.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al procesar la solicitud: " + ex.Message);
                var m = new Mascota { Id = id };
                return View(m);
            }
        }
    }
}