using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Integrador.Validations;

namespace Integrador.Models
{
    /// <summary>
    /// Partial class para extender Campanias con validaciones
    /// </summary>
    [MetadataType(typeof(CampaniasMetadata))]
    public partial class Campanias : IValidatableObject
    {
        /// <summary>
        /// Indica si la campaña está vigente (activa y dentro del rango de fechas)
        /// </summary>
        [NotMapped]
        public bool EstaVigente
        {
            get
            {
                if (!Activa) return false;
                var hoy = DateTime.Today;
                if (hoy < FechaInicio.Date) return false;
                if (FechaFin.HasValue && hoy > FechaFin.Value.Date) return false;
                return true;
            }
        }

        /// <summary>
        /// Días restantes de la campaña
        /// </summary>
        [NotMapped]
        public int? DiasRestantes
        {
            get
            {
                if (!FechaFin.HasValue) return null;
                var dias = (FechaFin.Value.Date - DateTime.Today).Days;
                return dias > 0 ? dias : 0;
            }
        }

        /// <summary>
        /// Validación personalizada para asegurar que FechaFin >= FechaInicio
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validar que FechaFin sea mayor o igual a FechaInicio
            if (FechaFin.HasValue && FechaFin.Value < FechaInicio)
            {
                yield return new ValidationResult(
                    "La fecha de fin debe ser mayor o igual a la fecha de inicio",
                    new[] { nameof(FechaFin) });
            }
        }
    }

    /// <summary>
    /// Metadatos para validaciones de Campanias
    /// </summary>
    public class CampaniasMetadata
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 100 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        [NotFutureDate(ErrorMessage = "La fecha de inicio no puede ser posterior a la fecha actual para campañas que ya iniciaron")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Activa")]
        public bool Activa { get; set; }

        [StringLength(300, ErrorMessage = "La URL de imagen no puede exceder 300 caracteres")]
        [Display(Name = "URL de Imagen")]
        public string ImagenUrl { get; set; }
    }
}
