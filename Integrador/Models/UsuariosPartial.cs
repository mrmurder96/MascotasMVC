using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        /// Nota: Requiere que el campo FechaNacimiento exista en la BD
        /// Descomentarcuando se ejecute el script CrearTablasNuevas.sql
        /// </summary>
        [NotMapped]
        public int Edad
        {
            get
            {
                // Descomentar cuando FechaNacimiento esté en BD:
                // if (FechaNacimiento.HasValue)
                // {
                //     var hoy = DateTime.Today;
                //     var edad = hoy.Year - FechaNacimiento.Value.Year;
                //     if (FechaNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
                //     return edad;
                // }
                return 18; // Por defecto mayor de edad hasta migrar BD
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
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string Email { get; set; }

        [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres")]
        public string Cedula { get; set; }

        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string Direccion { get; set; }
    }
}
