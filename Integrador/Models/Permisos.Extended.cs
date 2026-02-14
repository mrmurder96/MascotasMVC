using System.ComponentModel.DataAnnotations.Schema;

namespace Integrador.Models
{
    public partial class Permisos
    {
        /// <summary>
        /// Área a la que pertenece el controlador (ej: "Admin", null para área raíz)
        /// </summary>
        [NotMapped]
        public string Area { get; set; }
    }
}
