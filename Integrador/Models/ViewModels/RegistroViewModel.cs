using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Integrador.Validations;

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
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [StringLength(10, ErrorMessage = "La cédula no puede exceder 10 caracteres")]
        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [StringLength(15, ErrorMessage = "El teléfono no puede exceder 15 caracteres")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [StringLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        [EdadEntre(ErrorMessage = "Debe tener entre 18 y 70 años. La fecha no puede ser hoy ni futura.")]
        public DateTime? FechaNacimiento { get; set; }

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
