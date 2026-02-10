using System;

namespace Integrador.Models
{
    public class Campania
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Activa { get; set; }
        public string ImagenUrl { get; set; }
    }
}