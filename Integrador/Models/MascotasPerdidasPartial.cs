using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Integrador.Validations;

namespace Integrador.Models
{
    /// <summary>
    /// Partial class para extender MascotasPerdidas con propiedades de compatibilidad
    /// </summary>
    [MetadataType(typeof(MascotasPerdidasMetadata))]
    public partial class MascotasPerdidas
    {
        /// <summary>
        /// Propiedad de compatibilidad - indica si la mascota fue encontrada
        /// </summary>
        [NotMapped]
        public bool Encontrada
        {
            get => Estado == "Resuelto" || Estado == "Cerrado";
            set
            {
                if (value)
                {
                    Estado = "Resuelto";
                    FechaResolucion = DateTime.Now;
                }
                else
                {
                    Estado = "Activo";
                }
            }
        }

        /// <summary>
        /// Fecha en que fue encontrada (alias de FechaResolucion)
        /// </summary>
        [NotMapped]
        public DateTime? FechaEncontrada
        {
            get => FechaResolucion;
            set => FechaResolucion = value;
        }

        /// <summary>
        /// Color de la mascota (se almacena en Notas por ahora)
        /// </summary>
        [NotMapped]
        public string Color { get; set; }

        /// <summary>
        /// Aliases para compatibilidad con vistas
        /// </summary>
        [NotMapped]
        public string Tipo
        {
            get => TipoMascota;
            set => TipoMascota = value;
        }

        [NotMapped]
        public string Ubicacion
        {
            get => UbicacionReporte;
            set => UbicacionReporte = value;
        }

        [NotMapped]
        public string NombreContacto
        {
            get => NombreReportante;
            set => NombreReportante = value;
        }

        [NotMapped]
        public string Telefono
        {
            get => TelefonoContacto;
            set => TelefonoContacto = value;
        }

        [NotMapped]
        public string Email
        {
            get => EmailContacto;
            set => EmailContacto = value;
        }

        [NotMapped]
        public string Imagen
        {
            get => FotoUrl;
            set => FotoUrl = value;
        }
    }

    /// <summary>
    /// Metadatos para validaciones
    /// </summary>
    public class MascotasPerdidasMetadata
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo de reporte es requerido")]
        public string TipoReporte { get; set; }

        [Required(ErrorMessage = "El tipo de mascota es requerido")]
        public string TipoMascota { get; set; }

        [Required(ErrorMessage = "La ubicación es requerida")]
        public string UbicacionReporte { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El nombre del reportante es requerido")]
        public string NombreReportante { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string TelefonoContacto { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string EmailContacto { get; set; }

        [Required(ErrorMessage = "La fecha de reporte es requerida")]
        [ReportDate(MaxDaysAgo = 365, ErrorMessage = "La fecha de reporte no puede ser futura ni mayor a 1 año atrás")]
        [Display(Name = "Fecha de Reporte")]
        public DateTime FechaReporte { get; set; }

        [NotFutureDate(ErrorMessage = "La fecha de resolución no puede ser futura")]
        [Display(Name = "Fecha de Resolución")]
        public DateTime? FechaResolucion { get; set; }
    }
}
