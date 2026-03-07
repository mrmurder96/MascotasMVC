using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Integrador.Validations;

namespace Integrador.Models.ViewModels
{
    public class PerfilViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese nombres")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 50 caracteres")]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingrese apellidos")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 50 caracteres")]
        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Ingrese email")]
        [EmailAddress(ErrorMessage = "Email no vùlido")]
        [StringLength(50, ErrorMessage = "El email no puede exceder 50 caracteres")]
        [Display(Name = "Correo electrùnico")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "El telùfono no puede exceder 15 caracteres")]
        [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "El telùfono solo puede contener nùmeros, espacios, guiones y parùntesis")]
        [Display(Name = "Telùfono")]
        public string Telefono { get; set; }

        [StringLength(10, ErrorMessage = "La cùdula no puede exceder 10 caracteres")]
        [Display(Name = "Cùdula")]
        public string Cedula { get; set; }

        [StringLength(250, ErrorMessage = "La direcciùn no puede exceder 250 caracteres")]
        [Display(Name = "Direcciùn")]
        public string Direccion { get; set; }

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        [EdadEntre(ErrorMessage = "Debe tener entre 18 y 70 aÒos. La fecha no puede ser hoy ni futura.")]
        public DateTime? FechaNacimiento { get; set; }

        // Url a la imagen de perfil (puede ser /Account/ObtenerImagen?id=...)
        public string FotoPerfilRuta { get; set; }

        // Para enlazar los archivos subidos desde el formulario de perfil
        [Display(Name = "Subir nuevas fotos")]
        public HttpPostedFileBase[] NuevasFotos { get; set; }
    }
}