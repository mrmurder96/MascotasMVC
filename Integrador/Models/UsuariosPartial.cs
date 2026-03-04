using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Integrador.Validations;

namespace Integrador.Models
{
    /// <summary>
    /// Partial class para agregar propiedades y métodos adicionales a Usuarios sin modificar el archivo autogenerado
    /// </summary>
    [MetadataType(typeof(UsuariosMetadata))]
    public partial class Usuarios
    {
        /// <summary>
        /// Propiedad computada para nombre completo
        /// </summary>
        [NotMapped]
        public string Nombre => $"{Nombres} {Apellidos}".Trim();

        // Propiedad no mapeada para validar cambio de contraseña cada 90 días (RF-05)
        [NotMapped]
        public bool RequiereCambioPassword
        {
            get
            {
                // Si FechaUltimoCambioPassword está en la BD, usar esa lógica
                // Por ahora, usamos FechaRegistro como referencia
                var diasDesdeRegistro = (DateTime.Now - FechaRegistro).Days;
                return diasDesdeRegistro >= 90;
            }
        }

        /// <summary>
        /// Valida si el usuario es mayor de edad (RF-20)
        /// </summary>
        [NotMapped]
        public int Edad
        {
            get
            {
                if (FechaNacimiento.HasValue)
                {
                    var hoy = DateTime.Today;
                    var edad = hoy.Year - FechaNacimiento.Value.Year;
                    if (FechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
                    return edad;
                }
                return 18; // Por defecto mayor de edad si no tiene fecha
            }
        }

        [NotMapped]
        public bool EsMayorDeEdad => Edad >= 18;
    }

    /// <summary>
    /// Clase de metadatos para validaciones adicionales
    /// </summary>
    public class UsuariosMetadata
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 50 caracteres")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(50, ErrorMessage = "El email no puede exceder 50 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "La cédula no puede exceder 15 caracteres")]
        [RegularExpression(@"^[\d\-]+$", ErrorMessage = "La cédula solo puede contener números y guiones")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
        [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "El teléfono solo puede contener números, espacios, guiones y paréntesis")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [BirthDateRange(MaxYearsAgo = 120, ErrorMessage = "La fecha de nacimiento debe ser válida (no futura y no mayor a 120 años)")]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [NotFutureDate(ErrorMessage = "La fecha de registro no puede ser futura")]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }
    }
}
