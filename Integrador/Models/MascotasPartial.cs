using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Integrador.Validations;

namespace Integrador.Models
{
    /// <summary>
    /// Partial class para extender Mascotas con propiedades calculadas
    /// </summary>
    [MetadataType(typeof(MascotasMetadata))]
    public partial class Mascotas
    {
        /// <summary>
        /// Verifica si la mascota está disponible para adopción (RF-21)
        /// </summary>
        [NotMapped]
        public bool EstaDisponible => Estado?.Equals("Disponible", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Retorna el icono según el tipo de mascota (para vistas)
        /// </summary>
        [NotMapped]
        public string IconoTipo
        {
            get
            {
                if (string.IsNullOrEmpty(Tipo)) return "??";
                
                switch (Tipo.ToLower())
                {
                    case "perro":
                        return "??";
                    case "gato":
                        return "??";
                    case "ave":
                    case "pájaro":
                        return "??";
                    case "conejo":
                        return "??";
                    case "roedor":
                        return "??";
                    default:
                        return "??";
                }
            }
        }

        /// <summary>
        /// Descripción corta para listados
        /// </summary>
        [NotMapped]
        public string DescripcionCorta
        {
            get
            {
                if (string.IsNullOrEmpty(Descripcion)) return "Sin descripción";
                return Descripcion.Length > 100 ? Descripcion.Substring(0, 97) + "..." : Descripcion;
            }
        }

        /// <summary>
        /// Propiedad para manejar la imagen de la mascota (alias)
        /// </summary>
        [NotMapped]
        public string Imagen
        {
            get => FotoUrl;
            set => FotoUrl = value;
        }

        // Nota: Las propiedades Raza, Sexo, Tamano, FechaCreacion están definidas en el EDMX
        // y se generan automáticamente en Mascotas.cs

        /// <summary>
        /// Navegación a Categoría (alias para compatibilidad)
        /// </summary>
        [NotMapped]
        public Categoria Categoria
        {
            get => Categorias != null ? new Categoria
            {
                Id = Categorias.Id,
                Nombre = Categorias.Nombre,
                Descripcion = Categorias.Descripcion
            } : null;
        }

        /// <summary>
        /// Navegación a Refugio (alias para compatibilidad)
        /// </summary>
        [NotMapped]
        public Refugio Refugio
        {
            get => Refugios != null ? new Refugio
            {
                Id = Refugios.Id,
                Nombre = Refugios.Nombre,
                Descripcion = Refugios.Descripcion,
                Telefono = Refugios.Telefono,
                Email = Refugios.Email,
                Direccion = Refugios.Direccion
            } : null;
        }

        /// <summary>
        /// Alias para fecha de registro
        /// </summary>
        [NotMapped]
        public DateTime FechaRegistro { get; set; }
    }

    /// <summary>
    /// Clase de metadatos para validaciones
    /// </summary>
    public class MascotasMetadata
    {
        [Required(ErrorMessage = "El nombre de la mascota es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo de mascota es requerido")]
        [StringLength(30, ErrorMessage = "El tipo no puede exceder 30 caracteres")]
        [Display(Name = "Tipo de mascota")]
        public string Tipo { get; set; }

        [Range(0, 30, ErrorMessage = "La edad debe estar entre 0 y 30 años")]
        [Display(Name = "Edad (años)")]
        public int? Edad { get; set; }

        [StringLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(30, ErrorMessage = "El estado no puede exceder 30 caracteres")]
        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [StringLength(50, ErrorMessage = "La raza no puede exceder 50 caracteres")]
        [Display(Name = "Raza")]
        public string Raza { get; set; }

        [StringLength(20, ErrorMessage = "El sexo no puede exceder 20 caracteres")]
        [Display(Name = "Sexo")]
        public string Sexo { get; set; }

        [StringLength(20, ErrorMessage = "El tamaño no puede exceder 20 caracteres")]
        [Display(Name = "Tamaño")]
        public string Tamano { get; set; }

        [NotFutureDate(ErrorMessage = "La fecha de creación no puede ser futura")]
        [Display(Name = "Fecha de Registro")]
        public DateTime? FechaCreacion { get; set; }
    }
}

