using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Integrador.Models.ViewModels
{
    public class PerfilViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingrese apellidos")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Ingrese email")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        // Url a la imagen de perfil (puede ser /Account/ObtenerImagen?id=...)
        public string FotoPerfilRuta { get; set; }

        // Para enlazar los archivos subidos desde el formulario de perfil
        public HttpPostedFileBase[] NuevasFotos { get; set; }
    }
}