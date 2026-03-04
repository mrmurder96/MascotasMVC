using System;
using System.Linq;
using Integrador.Models;

namespace Integrador.Helpers
{
    /// <summary>
    /// Servicio centralizado para crear notificaciones en el sistema
    /// </summary>
    public static class NotificacionHelper
    {
        /// <summary>
        /// Crea una notificación para un usuario específico
        /// </summary>
        public static void CrearNotificacion(adopEntities db, int usuarioId, string titulo, string mensaje)
        {
            try
            {
                var notificacion = new Notificaciones
                {
                    UsuarioId = usuarioId,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    Leido = false,
                    Fecha = DateTime.Now
                };

                db.Notificaciones.Add(notificacion);
                db.SaveChanges();
            }
            catch
            {
                // Log error pero no fallar la operación principal
            }
        }

        /// <summary>
        /// Notificación de bienvenida para nuevos usuarios
        /// </summary>
        public static void NotificarBienvenida(adopEntities db, int usuarioId, string nombreUsuario)
        {
            CrearNotificacion(db, usuarioId,
                "🎉 ¡Bienvenido/a a Pura Compañía!",
                $"Hola {nombreUsuario}, nos alegra tenerte en nuestra comunidad. " +
                "Explora las mascotas disponibles para adopción y encuentra a tu compañero perfecto. " +
                "Si tienes alguna duda, visita nuestra sección de Soporte.");
        }

        /// <summary>
        /// Notificación cuando se aprueba una adopción
        /// </summary>
        public static void NotificarAdopcionAprobada(adopEntities db, int usuarioId, string nombreMascota)
        {
            CrearNotificacion(db, usuarioId,
                $"🎉 ¡Adopción Aprobada! - {nombreMascota}",
                $"¡Felicidades! Tu solicitud para adoptar a {nombreMascota} ha sido APROBADA. " +
                "Nos pondremos en contacto contigo en las próximas 48 horas para coordinar los siguientes pasos.");
        }

        /// <summary>
        /// Notificación cuando se rechaza una adopción
        /// </summary>
        public static void NotificarAdopcionRechazada(adopEntities db, int usuarioId, string nombreMascota, string motivo)
        {
            CrearNotificacion(db, usuarioId,
                $"Actualización de solicitud - {nombreMascota}",
                $"Lamentamos informarte que tu solicitud para adoptar a {nombreMascota} no ha sido aprobada. " +
                $"Motivo: {motivo}. Te invitamos a explorar otras mascotas disponibles.");
        }

        /// <summary>
        /// Notificación de nuevo seguimiento de mascota adoptada
        /// </summary>
        public static void NotificarNuevoSeguimiento(adopEntities db, int usuarioId, string nombreMascota, string estadoMascota, string tipoSeguimiento)
        {
            CrearNotificacion(db, usuarioId,
                $"📋 Seguimiento realizado - {nombreMascota}",
                $"Se ha realizado un seguimiento ({tipoSeguimiento}) de tu mascota adoptada {nombreMascota}. " +
                $"Estado actual: {estadoMascota}. Puedes ver los detalles en la sección 'Mis Adopciones'.");
        }

        /// <summary>
        /// Notificación cuando la mascota perdida ha sido marcada como encontrada
        /// </summary>
        public static void NotificarMascotaEncontrada(adopEntities db, int usuarioId, string nombreMascota)
        {
            CrearNotificacion(db, usuarioId,
                $"🎊 ¡Mascota encontrada! - {nombreMascota}",
                $"¡Excelentes noticias! El reporte de tu mascota perdida '{nombreMascota}' ha sido marcado como resuelto. " +
                "Nos alegra que hayan podido reencontrarse.");
        }

        /// <summary>
        /// Notificación de nuevo reporte de mascota perdida/encontrada en la zona
        /// </summary>
        public static void NotificarNuevoReportePerdida(adopEntities db, int usuarioId, string tipoReporte, string ubicacion, string descripcion)
        {
            string titulo = tipoReporte == "Perdida" 
                ? "🔍 Nueva mascota perdida reportada" 
                : "🐾 Nueva mascota encontrada reportada";
            
            CrearNotificacion(db, usuarioId, titulo,
                $"Se ha reportado una mascota {tipoReporte.ToLower()} cerca de {ubicacion}. " +
                $"Descripción: {descripcion}. Revisa si coincide con alguna mascota que conozcas.");
        }

        /// <summary>
        /// Notificación de nueva campaña de adopción
        /// </summary>
        public static void NotificarNuevaCampana(adopEntities db, int usuarioId, string tituloCampana, DateTime fechaInicio)
        {
            CrearNotificacion(db, usuarioId,
                $"📢 Nueva campaña: {tituloCampana}",
                $"¡Tenemos una nueva campaña de adopción! '{tituloCampana}' comienza el " +
                $"{fechaInicio.ToString("dd/MM/yyyy")}. No te pierdas esta oportunidad de darle un hogar a una mascota.");
        }

        /// <summary>
        /// Notificación cuando la solicitud pasa a revisión
        /// </summary>
        public static void NotificarSolicitudEnRevision(adopEntities db, int usuarioId, string nombreMascota)
        {
            CrearNotificacion(db, usuarioId,
                $"📝 Solicitud en revisión - {nombreMascota}",
                $"Tu solicitud de adopción para {nombreMascota} está siendo revisada por nuestro equipo. " +
                "Te notificaremos cuando tengamos una decisión. ¡Gracias por tu paciencia!");
        }

        /// <summary>
        /// Notificación de adopción finalizada/completada
        /// </summary>
        public static void NotificarAdopcionFinalizada(adopEntities db, int usuarioId, string nombreMascota)
        {
            CrearNotificacion(db, usuarioId,
                $"✨ ¡Adopción completada! - {nombreMascota}",
                $"¡Felicidades! El proceso de adopción de {nombreMascota} ha sido completado exitosamente. " +
                "Gracias por darle un hogar a esta mascota. Recuerda que haremos seguimientos periódicos.");
        }

        /// <summary>
        /// Notificación cuando el perfil ha sido actualizado
        /// </summary>
        public static void NotificarPerfilActualizado(adopEntities db, int usuarioId)
        {
            CrearNotificacion(db, usuarioId,
                "✅ Perfil actualizado",
                "Tu información de perfil ha sido actualizada correctamente. " +
                "Si no realizaste este cambio, por favor contacta con soporte inmediatamente.");
        }

        /// <summary>
        /// Notifica a todos los usuarios ciudadanos activos sobre una nueva campaña
        /// </summary>
        public static void NotificarCampanaATodos(adopEntities db, string tituloCampana, DateTime fechaInicio)
        {
            try
            {
                var usuariosActivos = db.Usuarios
                    .Where(u => u.EstaActivo && u.Rol == "Ciudadano")
                    .Select(u => u.Id)
                    .ToList();

                foreach (var usuarioId in usuariosActivos)
                {
                    var notificacion = new Notificaciones
                    {
                        UsuarioId = usuarioId,
                        Titulo = $"📢 Nueva campaña: {tituloCampana}",
                        Mensaje = $"¡Tenemos una nueva campaña de adopción! '{tituloCampana}' comienza el " +
                                  $"{fechaInicio.ToString("dd/MM/yyyy")}. No te pierdas esta oportunidad.",
                        Leido = false,
                        Fecha = DateTime.Now
                    };
                    db.Notificaciones.Add(notificacion);
                }
                db.SaveChanges();
            }
            catch
            {
                // Log error
            }
        }
    }
}
