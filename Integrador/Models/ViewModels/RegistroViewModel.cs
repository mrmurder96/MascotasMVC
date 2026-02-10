using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Integrador.Models.ViewModels
{
    public class RegistroViewModel
    {
        public RegistroViewModel()
        {
        }

        [Required(ErrorMessage = "Ingrese nombres")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Ingrese apellidos")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Ingrese email")]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ingrese contraseña")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirme contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        // Cambiado a public set para que el model binder pueda poblar los archivos subidos
        public HttpPostedFileBase[] Fotos { get; set; }
    }
}
