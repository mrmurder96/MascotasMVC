using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Integrador.Controllers;
using Integrador.Filters;
using Integrador.Models;

namespace Integrador.Areas.Admin.Controllers
{
    /// <summary>
    /// Controlador para todos los reportes del sistema (RF-22 a RF-28)
    /// </summary>
    [AdminAuthorize]
    [CargarPermisos]
    public class ReportesController : Controller
    {
        private adopEntities db = new adopEntities();

        // GET: Admin/Reportes
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult Index()
        {
            // Dashboard de reportes
            ViewBag.TotalMascotas = db.Mascotas.Count();
            ViewBag.MascotasDisponibles = db.Mascotas.Count(m => m.Estado == "Disponible");
            ViewBag.MascotasAdoptadas = db.Mascotas.Count(m => m.Estado == "Adoptado");
            ViewBag.TotalAdopciones = db.Adopciones.Count();
            ViewBag.AdopcionesPendientes = db.Adopciones.Count(a => a.Estado == "Pendiente" || a.Estado == "En Revisión");
            ViewBag.AdopcionesAprobadas = db.Adopciones.Count(a => a.Estado == "Aprobada" || a.Estado == "Completada");
            ViewBag.TotalUsuarios = db.Usuarios.Count(u => u.EstaActivo);
            ViewBag.UsuariosNuevosEsteMes = db.Usuarios.Count(u => u.EstaActivo && 
                u.FechaRegistro.Year == DateTime.Now.Year && 
                u.FechaRegistro.Month == DateTime.Now.Month);

            return View();
        }

        // GET: Admin/Reportes/Adopciones (RF-22)
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult Adopciones(DateTime? fechaDesde, DateTime? fechaHasta, string estado, string formato)
        {
            // Establecer fechas por defecto (último mes)
            if (!fechaDesde.HasValue)
                fechaDesde = DateTime.Now.AddMonths(-1);

            if (!fechaHasta.HasValue)
                fechaHasta = DateTime.Now;

            var adopciones = db.Adopciones
                .Include(a => a.Mascotas)
                .Where(a => a.FechaSolicitud >= fechaDesde && a.FechaSolicitud <= fechaHasta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                adopciones = adopciones.Where(a => a.Estado == estado);
            }

            var listaAdopciones = adopciones.OrderByDescending(a => a.FechaSolicitud).ToList();

            // Estadísticas del periodo
            ViewBag.TotalAdopciones = listaAdopciones.Count;
            ViewBag.Aprobadas = listaAdopciones.Count(a => a.Estado == "Aprobada" || a.Estado == "Completada");
            ViewBag.Pendientes = listaAdopciones.Count(a => a.Estado == "Pendiente" || a.Estado == "En Revisión");
            ViewBag.Rechazadas = listaAdopciones.Count(a => a.Estado == "Rechazada");
            ViewBag.TasaAprobacion = listaAdopciones.Count > 0 
                ? Math.Round((decimal)ViewBag.Aprobadas / listaAdopciones.Count * 100, 2) 
                : 0;

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.EstadoSeleccionado = estado;
            ViewBag.Estados = new SelectList(new[] { "Todos", "Pendiente", "En Revisión", "Aprobada", "Rechazada", "Completada", "Cancelada" });

            // Si solicitan exportación
            if (!string.IsNullOrEmpty(formato))
            {
                return ExportarAdopciones(listaAdopciones, formato, fechaDesde.Value, fechaHasta.Value);
            }

            return View(listaAdopciones);
        }

        // GET: Admin/Reportes/Mascotas (RF-23)
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult Mascotas(string tipo, string estado, string formato)
        {
            var mascotas = db.Mascotas.AsQueryable();

            if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
            {
                mascotas = mascotas.Where(m => m.Tipo == tipo);
            }

            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                mascotas = mascotas.Where(m => m.Estado == estado);
            }

            var listaMascotas = mascotas.OrderBy(m => m.Nombre).ToList();

            // Estadísticas
            ViewBag.TotalMascotas = listaMascotas.Count;
            ViewBag.Disponibles = listaMascotas.Count(m => m.Estado == "Disponible");
            ViewBag.Adoptadas = listaMascotas.Count(m => m.Estado == "Adoptado");
            ViewBag.EnProceso = listaMascotas.Count(m => m.Estado == "En Proceso");

            // Estadísticas por tipo
            ViewBag.EstadisticasPorTipo = listaMascotas
                .GroupBy(m => m.Tipo)
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // Estadísticas por edad
            ViewBag.EdadPromedio = listaMascotas.Where(m => m.Edad.HasValue).Average(m => m.Edad.Value);

            ViewBag.TipoSeleccionado = tipo;
            ViewBag.EstadoSeleccionado = estado;
            ViewBag.Tipos = new SelectList(new[] { "Todos", "Perro", "Gato", "Ave", "Conejo", "Roedor", "Otro" });
            ViewBag.Estados = new SelectList(new[] { "Todos", "Disponible", "Adoptado", "En Proceso", "No Disponible" });

            if (!string.IsNullOrEmpty(formato))
            {
                return ExportarMascotas(listaMascotas, formato);
            }

            return View(listaMascotas);
        }

        // GET: Admin/Reportes/Solicitudes (RF-24)
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult Solicitudes(string estado, DateTime? fechaDesde, DateTime? fechaHasta, string formato)
        {
            if (!fechaDesde.HasValue)
                fechaDesde = DateTime.Now.AddMonths(-3);

            if (!fechaHasta.HasValue)
                fechaHasta = DateTime.Now;

            var solicitudes = db.Adopciones
                .Include(a => a.Mascotas)
                .Where(a => a.FechaSolicitud >= fechaDesde && a.FechaSolicitud <= fechaHasta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                solicitudes = solicitudes.Where(s => s.Estado == estado);
            }

            var listaSolicitudes = solicitudes.OrderByDescending(s => s.FechaSolicitud).ToList();

            // Análisis por estado
            ViewBag.PorEstado = listaSolicitudes
                .GroupBy(s => s.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count(), Porcentaje = Math.Round((decimal)g.Count() / listaSolicitudes.Count * 100, 2) })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // Tiempo promedio de procesamiento
            var solicitudesAprobadas = listaSolicitudes.Where(s => s.Estado == "Aprobada" || s.Estado == "Completada").ToList();
            ViewBag.TiempoPromedioAprobacion = solicitudesAprobadas.Any() 
                ? Math.Round(solicitudesAprobadas.Average(s => (DateTime.Now - s.FechaSolicitud).TotalDays), 2)
                : 0;

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.EstadoSeleccionado = estado;
            ViewBag.Estados = new SelectList(new[] { "Todos", "Pendiente", "En Revisión", "Aprobada", "Rechazada", "Completada", "Cancelada" });

            if (!string.IsNullOrEmpty(formato))
            {
                return ExportarSolicitudes(listaSolicitudes, formato, fechaDesde.Value, fechaHasta.Value);
            }

            return View(listaSolicitudes);
        }

        // GET: Admin/Reportes/Usuarios (RF-25)
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult Usuarios(string rol, bool? soloActivos, string formato)
        {
            var usuarios = db.Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(rol) && rol != "Todos")
            {
                usuarios = usuarios.Where(u => u.Rol == rol);
            }

            if (soloActivos.HasValue && soloActivos.Value)
            {
                usuarios = usuarios.Where(u => u.EstaActivo);
            }

            var listaUsuarios = usuarios.OrderByDescending(u => u.FechaRegistro).ToList();

            // Estadísticas
            ViewBag.TotalUsuarios = listaUsuarios.Count;
            ViewBag.UsuariosActivos = listaUsuarios.Count(u => u.EstaActivo);
            ViewBag.UsuariosBloqueados = listaUsuarios.Count(u => u.Bloqueado);

            // Por rol
            ViewBag.PorRol = listaUsuarios
                .GroupBy(u => u.Rol)
                .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                .ToList();

            // Registros por mes (últimos 6 meses)
            ViewBag.RegistrosPorMes = listaUsuarios
                .Where(u => u.FechaRegistro >= DateTime.Now.AddMonths(-6))
                .GroupBy(u => new { u.FechaRegistro.Year, u.FechaRegistro.Month })
                .Select(g => new { 
                    Mes = g.Key.Month + "/" + g.Key.Year, 
                    Cantidad = g.Count() 
                })
                .OrderBy(x => x.Mes)
                .ToList();

            ViewBag.RolSeleccionado = rol;
            ViewBag.SoloActivos = soloActivos;
            ViewBag.Roles = new SelectList(new[] { "Todos", "Administrador", "Ciudadano", "Staff" });

            if (!string.IsNullOrEmpty(formato))
            {
                return ExportarUsuarios(listaUsuarios, formato);
            }

            return View(listaUsuarios);
        }

        // GET: Admin/Reportes/PostSeguimiento
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult PostSeguimiento(DateTime? fechaDesde, DateTime? fechaHasta, string estadoMascota, string formato)
        {
            if (!fechaDesde.HasValue)
                fechaDesde = DateTime.Now.AddMonths(-6);

            if (!fechaHasta.HasValue)
                fechaHasta = DateTime.Now;

            var seguimientos = db.Seguimientos
                .Include("Adopciones")
                .Include("Adopciones.Mascotas")
                .Where(s => s.FechaSeguimiento >= fechaDesde && s.FechaSeguimiento <= fechaHasta)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estadoMascota) && estadoMascota != "Todos")
            {
                seguimientos = seguimientos.Where(s => s.EstadoMascota == estadoMascota);
            }

            var listaSeguimientos = seguimientos.OrderByDescending(s => s.FechaSeguimiento).ToList();

            // Estadisticas
            ViewBag.TotalSeguimientos = listaSeguimientos.Count;
            ViewBag.SeguimientosExcelente = listaSeguimientos.Count(s => s.EstadoMascota == "Excelente");
            ViewBag.SeguimientosBueno = listaSeguimientos.Count(s => s.EstadoMascota == "Bueno");
            ViewBag.SeguimientosRegular = listaSeguimientos.Count(s => s.EstadoMascota == "Regular");
            ViewBag.SeguimientosCritico = listaSeguimientos.Count(s => s.EstadoMascota == "Requiere Atencion" || s.EstadoMascota == "Critico");

            // Estadisticas por tipo de seguimiento
            ViewBag.PorTipo = listaSeguimientos
                .GroupBy(s => s.TipoSeguimiento ?? "Sin tipo")
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .OrderByDescending(x => x.Cantidad)
                .ToList();

            // Adopciones sin seguimiento reciente (ultimos 30 dias)
            var adopcionesAprobadas = db.Adopciones
                .Where(a => a.Estado == "Aprobada" || a.Estado == "Completada")
                .ToList();

            var fechaLimite = DateTime.Now.AddDays(-30);
            var adopcionesSinSeguimiento = adopcionesAprobadas
                .Where(a => !db.Seguimientos.Any(s => s.AdopcionId == a.Id && s.FechaSeguimiento >= fechaLimite))
                .Count();

            ViewBag.AdopcionesSinSeguimiento = adopcionesSinSeguimiento;
            ViewBag.TotalAdopcionesActivas = adopcionesAprobadas.Count;

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.EstadoMascotaSeleccionado = estadoMascota;
            ViewBag.EstadosMascota = new SelectList(new[] { "Todos", "Excelente", "Bueno", "Regular", "Requiere Atencion", "Critico" });

            if (!string.IsNullOrEmpty(formato) && formato == "csv")
            {
                return ExportarSeguimientos(listaSeguimientos, fechaDesde.Value, fechaHasta.Value);
            }

            return View(listaSeguimientos);
        }

        // GET: Admin/Reportes/Auditoria (RF-28)
        [ValidarPermisoCrud(ControllerName = "Reportes", Operacion = "Leer")]
        public ActionResult Auditoria(DateTime? fechaDesde, DateTime? fechaHasta, string formato)
        {
            if (!fechaDesde.HasValue)
                fechaDesde = DateTime.Now.AddMonths(-1);

            if (!fechaHasta.HasValue)
                fechaHasta = DateTime.Now;

            // Reporte de auditoría de seguridad
            var usuariosBloqueados = db.Usuarios
                .Where(u => u.Bloqueado && u.FechaRegistro >= fechaDesde && u.FechaRegistro <= fechaHasta)
                .OrderByDescending(u => u.FechaRegistro)
                .ToList();

            var usuariosConIntentos = db.Usuarios
                .Where(u => u.IntentosFallidos > 0)
                .OrderByDescending(u => u.IntentosFallidos)
                .Take(50)
                .ToList();

            ViewBag.TotalBloqueados = usuariosBloqueados.Count;
            ViewBag.TotalConIntentos = usuariosConIntentos.Count;
            ViewBag.UsuariosEnRiesgo = usuariosConIntentos.Count(u => u.IntentosFallidos >= 2);

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;

            var modelo = new {
                UsuariosBloqueados = usuariosBloqueados,
                UsuariosConIntentos = usuariosConIntentos
            };

            return View(modelo);
        }

        // Métodos de exportación
        private ActionResult ExportarAdopciones(List<Adopciones> adopciones, string formato, DateTime fechaDesde, DateTime fechaHasta)
        {
            if (formato == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Mascota,Solicitante,Email,Teléfono,Fecha Solicitud,Estado");

                foreach (var a in adopciones)
                {
                    csv.AppendLine($"{a.Id},{a.Mascotas?.Nombre},{a.NombreSolicitante},{a.Email},{a.Telefono},{a.FechaSolicitud:yyyy-MM-dd},{a.Estado}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Adopciones_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.csv");
            }

            return RedirectToAction("Adopciones");
        }

        private ActionResult ExportarMascotas(List<Mascotas> mascotas, string formato)
        {
            if (formato == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Nombre,Tipo,Edad,Ubicación,Estado");

                foreach (var m in mascotas)
                {
                    csv.AppendLine($"{m.Id},{m.Nombre},{m.Tipo},{m.Edad},{m.Ubicacion},{m.Estado}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Mascotas_{DateTime.Now:yyyyMMdd}.csv");
            }

            return RedirectToAction("Mascotas");
        }

        private ActionResult ExportarSolicitudes(List<Adopciones> solicitudes, string formato, DateTime fechaDesde, DateTime fechaHasta)
        {
            if (formato == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Mascota,Solicitante,Email,Fecha,Estado");

                foreach (var s in solicitudes)
                {
                    csv.AppendLine($"{s.Id},{s.Mascotas?.Nombre},{s.NombreSolicitante},{s.Email},{s.FechaSolicitud:yyyy-MM-dd},{s.Estado}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Solicitudes_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.csv");
            }

            return RedirectToAction("Solicitudes");
        }

        private ActionResult ExportarUsuarios(List<Usuarios> usuarios, string formato)
        {
            if (formato == "csv")
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("ID,Nombre Completo,Email,Rol,Fecha Registro,Activo,Bloqueado");

                foreach (var u in usuarios)
                {
                    csv.AppendLine($"{u.Id},{u.Nombres} {u.Apellidos},{u.Email},{u.Rol},{u.FechaRegistro:yyyy-MM-dd},{u.EstaActivo},{u.Bloqueado}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Usuarios_{DateTime.Now:yyyyMMdd}.csv");
            }

            return RedirectToAction("Usuarios");
        }

        private ActionResult ExportarSeguimientos(List<Seguimientos> seguimientos, DateTime fechaDesde, DateTime fechaHasta)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Adopcion ID,Mascota,Tipo Seguimiento,Fecha,Estado Mascota,Observaciones");

            foreach (var s in seguimientos)
            {
                var nombreMascota = s.Adopciones?.Mascotas?.Nombre ?? "N/A";
                csv.AppendLine($"{s.Id},{s.AdopcionId},{nombreMascota},{s.TipoSeguimiento},{s.FechaSeguimiento:yyyy-MM-dd},{s.EstadoMascota},{s.Observaciones?.Replace(",", " ")}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"PostSeguimiento_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.csv");
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
