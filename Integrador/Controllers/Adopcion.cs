using System;

namespace Integrador.Models
{
    public class Adopcion
    {
        public int Id { get; set; }
        public int MascotaId { get; set; }
        public int? UsuarioId { get; set; }
        public string NombreSolicitante { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; }
    }
}