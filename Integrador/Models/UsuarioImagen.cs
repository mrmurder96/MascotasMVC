using System;

namespace Integrador.Models
{
    // POCO para referencias internas (no ligado automáticamente al EDMX)
    public class UsuarioImagen
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombreArchivo { get; set; }
        public string ContentType { get; set; }
        public byte[] Data { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}