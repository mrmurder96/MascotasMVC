using System.ComponentModel.DataAnnotations.Schema;

namespace Integrador.Models
{
    public partial class Categorias
    {
        [NotMapped]
        public bool EsActivo
        {
            get => EstaActiva;
            set => EstaActiva = value;
        }

        [NotMapped]
        public int CantidadMascotas
        {
            get
            {
                return Mascotas != null ? Mascotas.Count : 0;
            }
        }
    }
}
