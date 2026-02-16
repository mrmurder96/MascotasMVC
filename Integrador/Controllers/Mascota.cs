using System;

namespace Integrador.Models
{
    public class Mascota
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int Edad { get; set; }
        public string Ubicacion { get; set; }
        public string Descripcion { get; set; }
        public string FotoUrl { get; set; }
        public string Estado { get; set; }
        public string Raza { get; set; }
        public string Sexo { get; set; }
        public string Tamano { get; set; }
    }
}