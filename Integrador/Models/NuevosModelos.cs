using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Integrador.Models
{
    /// <summary>
    /// Modelo para Categorías de Mascotas (RF-12)
    /// Este modelo se creará posteriormente en la BD, por ahora es un modelo POCO
    /// </summary>
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        [StringLength(50, ErrorMessage = "El icono no puede exceder 50 caracteres")]
        public string Icono { get; set; } // Emoji o clase de icono

        public bool EstaActiva { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int Orden { get; set; }

        // Propiedades de navegación
        // public virtual ICollection<Mascotas> Mascotas { get; set; }

        // Propiedades computadas para compatibilidad con vistas
        [NotMapped]
        public bool EsActivo
        {
            get => EstaActiva;
            set => EstaActiva = value;
        }

        [NotMapped]
        public int CantidadMascotas { get; set; } // Se llenará desde el controlador
    }

    /// <summary>
    /// Modelo para Seguimiento Post-Adopción (RF-18)
    /// </summary>
    [Table("Seguimientos")]
    public class Seguimiento
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La adopción es requerida")]
        public int AdopcionId { get; set; }

        [Required(ErrorMessage = "La fecha de seguimiento es requerida")]
        public DateTime FechaSeguimiento { get; set; }

        [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
        public string TipoSeguimiento { get; set; } // Visita, Llamada, Email

        [StringLength(2000, ErrorMessage = "Las observaciones no pueden exceder 2000 caracteres")]
        public string Observaciones { get; set; }

        [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
        public string EstadoMascota { get; set; } // Excelente, Bueno, Regular, Malo

        public int? UsuarioRealizaSeguimientoId { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? ProximoSeguimiento { get; set; }

        // Navegación
        [ForeignKey("AdopcionId")]
        public virtual Adopciones Adopcion { get; set; }
    }

    /// <summary>
    /// Modelo para Mascotas Perdidas/Encontradas (RF-19)
    /// </summary>
    [Table("MascotasPerdidas")]
    public class MascotaPerdida
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo de reporte es requerido")]
        [StringLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres")]
        public string TipoReporte { get; set; } // "Perdida" o "Encontrada"

        [Required(ErrorMessage = "El tipo de mascota es requerido")]
        [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
        public string TipoMascota { get; set; } // Perro, Gato, etc.

        [StringLength(100, ErrorMessage = "La raza no puede exceder 100 caracteres")]
        public string Raza { get; set; }

        [Required(ErrorMessage = "La ubicación es requerida")]
        [StringLength(200, ErrorMessage = "La ubicación no puede exceder 200 caracteres")]
        public string UbicacionReporte { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; }

        [StringLength(500, ErrorMessage = "La URL de la foto no puede exceder 500 caracteres")]
        public string FotoUrl { get; set; }

        public DateTime FechaReporte { get; set; }

        [Required(ErrorMessage = "El nombre del reportante es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string NombreReportante { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string TelefonoContacto { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string EmailContacto { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres")]
        public string Estado { get; set; } // "Activo", "Resuelto", "Cerrado"

        public int? UsuarioRegistraId { get; set; }

        public DateTime? FechaResolucion { get; set; }

        [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
        public string Notas { get; set; }

        // Propiedades computadas
        [NotMapped]
        public bool EstaActivo => Estado == "Activo";

        [NotMapped]
        public bool EsPerdida => TipoReporte == "Perdida";

        [NotMapped]
        public bool EsEncontrada => TipoReporte == "Encontrada";

        // Aliases para compatibilidad con vistas
        [NotMapped]
        public string Tipo
        {
            get => TipoMascota;
            set => TipoMascota = value;
        }

        [NotMapped]
        public string Ubicacion
        {
            get => UbicacionReporte;
            set => UbicacionReporte = value;
        }

        [NotMapped]
        public string NombreContacto
        {
            get => NombreReportante;
            set => NombreReportante = value;
        }

        [NotMapped]
        public string Telefono
        {
            get => TelefonoContacto;
            set => TelefonoContacto = value;
        }

        [NotMapped]
        public string Email
        {
            get => EmailContacto;
            set => EmailContacto = value;
        }

        [NotMapped]
        public string Imagen
        {
            get => FotoUrl;
            set => FotoUrl = value;
        }
    }

    /// <summary>
    /// Modelo para Refugios/Asociaciones (RF-11)
    /// </summary>
    [Table("Refugios")]
    public class Refugio
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del refugio es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "El nombre del responsable no puede exceder 200 caracteres")]
        public string NombreResponsable { get; set; }

        [Range(-90, 90, ErrorMessage = "Latitud inválida")]
        public decimal? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud inválida")]
        public decimal? Longitud { get; set; }

        [StringLength(500, ErrorMessage = "La URL del sitio web no puede exceder 500 caracteres")]
        [Url(ErrorMessage = "Formato de URL inválido")]
        public string SitioWeb { get; set; }

        public int CapacidadMaxima { get; set; }

        public int MascotasActuales { get; set; }

        public bool EstaActivo { get; set; }

        public DateTime FechaRegistro { get; set; }

        [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
        public string Ciudad { get; set; }

        // Propiedades computadas
        [NotMapped]
        public int Disponibilidad => CapacidadMaxima - MascotasActuales;

        [NotMapped]
        public decimal PorcentajeOcupacion => CapacidadMaxima > 0 ? (decimal)MascotasActuales / CapacidadMaxima * 100 : 0;

        // Aliases para compatibilidad con vistas
        [NotMapped]
        public string Responsable
        {
            get => NombreResponsable;
            set => NombreResponsable = value;
        }

        [NotMapped]
        public bool EsActivo
        {
            get => EstaActivo;
            set => EstaActivo = value;
        }

        [NotMapped]
        public DateTime FechaCreacion
        {
            get => FechaRegistro;
            set => FechaRegistro = value;
        }

        [NotMapped]
        public int CantidadMascotas
        {
            get => MascotasActuales;
            set => MascotasActuales = value;
        }
    }
}
