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

        [Required(ErrorMessage = "Ingrese contraseña")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula y un número")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirme contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Fotos de identificación")]
        public HttpPostedFileBase[] Fotos { get; set; }
    }
}
