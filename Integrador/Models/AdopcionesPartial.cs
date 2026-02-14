using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Integrador.Models
{
    /// <summary>
    /// Partial class para extender Adopciones (Solicitudes)
    /// </summary>
    [MetadataType(typeof(AdopcionesMetadata))]
    public partial class Adopciones
    {
        /// <summary>
        /// Estados válidos de una solicitud de adopción (RF-17, RF-24)
        /// </summary>
        public static class Estados
        {
            public const string Pendiente = "Pendiente";
            public const string EnRevision = "En Revisión";
            public const string Aprobada = "Aprobada";
            public const string Rechazada = "Rechazada";
            public const string Completada = "Completada";
            public const string Finalizada = "Finalizada";
            public const string Cancelada = "Cancelada";
        }

        /// <summary>
        /// Propiedad de navegación al Usuario
        /// </summary>
        [NotMapped]
        public Usuarios Usuario
        {
            get
            {
                if (!UsuarioId.HasValue) return null;
                
                using (var db = new adopEntities())
                {
                    return db.Usuarios.Find(UsuarioId.Value);
                }
            }
        }

        /// <summary>
        /// Alias para la propiedad de navegación Mascota
        /// </summary>
        [NotMapped]
        public Mascotas Mascota => Mascotas;

        /// <summary>
        /// Fecha de aprobación (calculada o almacenada en campo adicional)
        /// </summary>
        [NotMapped]
        public DateTime? FechaAprobacion { get; set; }

        /// <summary>
        /// Fecha de finalización (calculada o almacenada en campo adicional)
        /// </summary>
        [NotMapped]
        public DateTime? FechaFinalizacion { get; set; }

        /// <summary>
        /// Comentarios adicionales sobre la adopción
        /// </summary>
        [NotMapped]
        public string Comentarios { get; set; }

        /// <summary>
        /// Motivo de rechazo o comentarios
        /// </summary>
        [NotMapped]
        public string Motivo { get; set; }

        /// <summary>
        /// Verifica si la solicitud está pendiente
        /// </summary>
        [NotMapped]
        public bool EstaPendiente => Estado == Estados.Pendiente || Estado == Estados.EnRevision;

        /// <summary>
        /// Verifica si la solicitud fue aprobada
        /// </summary>
        [NotMapped]
        public bool EstaAprobada => Estado == Estados.Aprobada || Estado == Estados.Completada || Estado == Estados.Finalizada;

        /// <summary>
        /// Retorna el color del badge según el estado
        /// </summary>
        [NotMapped]
        public string ColorEstado
        {
            get
            {
                switch (Estado)
                {
                    case Estados.Pendiente:
                        return "warning"; // Amarillo
                    case Estados.EnRevision:
                        return "info"; // Azul
                    case Estados.Aprobada:
                    case Estados.Completada:
                    case Estados.Finalizada:
                        return "success"; // Verde
                    case Estados.Rechazada:
                    case Estados.Cancelada:
                        return "danger"; // Rojo
                    default:
                        return "secondary"; // Gris
                }
            }
        }

        /// <summary>
        /// Días desde la solicitud
        /// </summary>
        [NotMapped]
        public int DiasDesdeRSolicitud => (DateTime.Now - FechaSolicitud).Days;
    }

    /// <summary>
    /// Clase de metadatos para validaciones
    /// </summary>
    public class AdopcionesMetadata
    {
        [Required(ErrorMessage = "La mascota es requerida")]
        public int MascotaId { get; set; }

        [Required(ErrorMessage = "El nombre del solicitante es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string NombreSolicitante { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
        public string Estado { get; set; }
    }
}
