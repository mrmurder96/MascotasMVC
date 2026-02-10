using System;

namespace Integrador.Models
{
    public class Centro
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Telefono { get; set; }
    }
}