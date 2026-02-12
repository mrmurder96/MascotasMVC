using System;
using System.Collections.Generic;

namespace Integrador.Models
{
    public partial class Permisos
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string Icono { get; set; }
        public int Orden { get; set; }
        public bool EsActivo { get; set; }
        
        // Indica si este permiso tiene operaciones CRUD
        public bool TieneCrud { get; set; }
    }

    public partial class RolPermisos
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public int PermisoId { get; set; }
        public bool TienePermiso { get; set; }

        public virtual Permisos Permiso { get; set; }
    }

    // Modelo para permisos CRUD granulares
    public partial class PermisoCrud
    {
        public int Id { get; set; }
        public string Rol { get; set; }
        public int PermisoId { get; set; }
        public string ControllerName { get; set; }
        
        // Permisos CRUD individuales
        public bool PuedeCrear { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeActualizar { get; set; }
        public bool PuedeEliminar { get; set; }
        
        public virtual Permisos Permiso { get; set; }
    }

    // ViewModel para la vista de gestión de permisos
    public class PermisoConCrudViewModel
    {
        public Permisos Permiso { get; set; }
        public PermisoCrud PermisosCrud { get; set; }
        public bool TieneAcceso { get; set; }
    }
}

