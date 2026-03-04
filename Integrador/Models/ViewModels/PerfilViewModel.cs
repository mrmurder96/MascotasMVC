using System.ComponentModel.DataAnnotations;
using System.Web;

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
        [EmailAddress(ErrorMessage = "Email no válido")]
        [StringLength(50, ErrorMessage = "El email no puede exceder 50 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
        [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "El teléfono solo puede contener números, espacios, guiones y paréntesis")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        // Url a la imagen de perfil (puede ser /Account/ObtenerImagen?id=...)
        public string FotoPerfilRuta { get; set; }

        // Para enlazar los archivos subidos desde el formulario de perfil
        [Display(Name = "Subir nuevas fotos")]
        public HttpPostedFileBase[] NuevasFotos { get; set; }
    }
}