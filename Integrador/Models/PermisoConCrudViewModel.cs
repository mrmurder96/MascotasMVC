using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Integrador.Models
{
    /// <summary>
    /// ViewModel para mostrar permisos con sus operaciones CRUD por rol
    /// Usado en la vista de gestión de permisos
    /// </summary>
    public class PermisoConCrudViewModel
    {
        public Permisos Permiso { get; set; }
        public PermisoCrud PermisoCrud { get; set; }
        public bool TieneAcceso { get; set; }
    }

    /// <summary>
    /// Clase auxiliar para representar los permisos CRUD (Crear, Leer, Actualizar, Eliminar)
    /// de un rol específico sobre un controlador
    /// </summary>
    public class PermisoCrud
    {
        public int PermisoId { get; set; }
        public string Rol { get; set; }
        public string ControllerName { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeActualizar { get; set; }
        public bool PuedeEliminar { get; set; }
    }
}
