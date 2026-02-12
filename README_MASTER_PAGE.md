# Sistema de Master Page con Gestión de Permisos

## ? Archivos Creados

### 1. **Master Page Principal**
- **Archivo**: `Integrador\Views\Shared\_LayoutMaster.cshtml`
- **Descripción**: Layout principal con navbar y sidebar dinámico que se adapta según permisos del usuario
- **Características**:
  - Navbar superior con información del usuario y botón de logout
  - Sidebar colapsable con menú de navegación
  - El menú se genera dinámicamente según los permisos del rol
  - Diseño responsive

### 2. **Modelo de Permisos**
- **Archivo**: `Integrador\Models\Permisos.cs`
- **Clases**:
  - `Permisos`: Define los permisos disponibles en el sistema
  - `RolPermisos`: Relaciona roles con permisos

### 3. **Controlador de Permisos**
- **Archivo**: `Integrador\Controllers\PermisosController.cs`
- **Funciones**:
  - `Index()`: Muestra la página de gestión de permisos
  - `ActualizarPermiso()`: Actualiza permisos de un rol (POST)
  - Solo accesible por Administradores

### 4. **Vista de Gestión de Permisos**
- **Archivo**: `Integrador\Views\Permisos\Index.cshtml`
- **Características**:
  - Interfaz visual para gestionar permisos por rol
  - Pestañas para cambiar entre Administrador y Ciudadano
  - Toggle switches para activar/desactivar permisos
  - Los permisos del Administrador son de solo lectura (siempre tienen acceso completo)

### 5. **Filtro de Carga de Permisos**
- **Archivo**: `Integrador\Filters\CargarPermisosAttribute.cs`
- **Descripción**: ActionFilter que carga automáticamente los permisos del usuario en ViewBag
- **Uso**: Aplicar `[CargarPermisos]` a los controladores

### 6. **Vistas Actualizadas**
- `Integrador\Views\Admin\Index.cshtml` - Usa la nueva Master Page
- `Integrador\Views\Ciudadano\Index.cshtml` - Usa la nueva Master Page
- `Integrador\Views\Ciudadano\PerfilMaster.cshtml` - Vista de perfil con Master Page
- `Integrador\Views\Ciudadano\NotificacionesMaster.cshtml` - Vista de notificaciones con Master Page

## ?? Cómo Usar

### Paso 1: Aplicar el Filtro a los Controladores
Los controladores `AdminController` y `CiudadanoController` ya tienen aplicado el filtro `[CargarPermisos]`:

```csharp
[AdminAuthorize]
[CargarPermisos]
public class AdminController : Controller
{
    // ...
}

[ClienteAuthorize]
[CargarPermisos]
public class CiudadanoController : Controller
{
    // ...
}
```

### Paso 2: Usar la Master Page en tus Vistas
Para cualquier vista que quieras que use el nuevo sistema de navegación:

```razor
@{
    ViewBag.Title = "Tu Título";
    Layout = "~/Views/Shared/_LayoutMaster.cshtml";
}

<!-- Tu contenido aquí -->
```

### Paso 3: Acceder a la Gestión de Permisos
1. Iniciar sesión como **Administrador**
2. Navegar a: `/Permisos/Index`
3. Seleccionar el rol "Ciudadano" en las pestañas
4. Activar/desactivar los permisos usando los switches
5. Los cambios se guardan automáticamente

## ?? Permisos Disponibles por Defecto

### Administrador (Acceso Completo)
- Dashboard
- Usuarios
- Mascotas
- Campañas
- Adopciones
- Centros
- Perfil
- Notificaciones
- Permisos

### Ciudadano (Configurable)
Por defecto tienen acceso a:
- Centros
- Perfil
- Notificaciones

El administrador puede agregar o quitar permisos adicionales.

## ?? Personalización

### Cambiar Iconos del Menú
Edita el archivo `Integrador\Filters\CargarPermisosAttribute.cs` o `Integrador\Controllers\PermisosController.cs`:

```csharp
new Permisos { 
    Id = 1, 
    Nombre = "Dashboard", 
    ControllerName = "Admin", 
    ActionName = "Index", 
    Icono = "??",  // Cambia este emoji o usa clases de FontAwesome
    Orden = 1, 
    EsActivo = true 
}
```

### Agregar Nuevos Permisos
1. Edita `ObtenerPermisosDisponibles()` en `PermisosController.cs`
2. Agrega el nuevo permiso con su información:

```csharp
new Permisos { 
    Id = 10, 
    Nombre = "Reportes", 
    ControllerName = "Reportes", 
    ActionName = "Index", 
    Icono = "??", 
    Orden = 10, 
    EsActivo = true 
}
```

### Cambiar Colores y Estilos
Modifica los estilos en `_LayoutMaster.cshtml`:
- `.sidebar`: Color del sidebar
- `.navbar-master`: Estilo del navbar
- `.menu-item`: Estilo de los items del menú

## ?? Notas Importantes

1. **Persistencia de Permisos**: Actualmente los permisos se guardan en Session (temporal). Para producción, deberías:
   - Crear las tablas `Permisos` y `RolPermisos` en la base de datos
   - Actualizar el método `ActualizarPermiso()` para guardar en BD
   - Actualizar `ObtenerPermisosPorRol()` para leer de BD

2. **Vistas Opcionales**: Se crearon vistas con sufijo "Master" (PerfilMaster, NotificacionesMaster) como alternativas. Puedes:
   - Reemplazar las vistas originales con estas
   - O mantener ambas versiones

3. **Área Admin**: Si el controlador Admin está en un área, asegúrate de especificarlo en las rutas:
   ```csharp
   return RedirectToAction("Index", "Admin", new { area = "Admin" });
   ```

## ?? Próximos Pasos Sugeridos

1. **Diseño personalizado**: Aplicar tu diseño específico a la Master Page
2. **Integración con BD**: Persistir permisos en base de datos
3. **Auditoría**: Log de cambios de permisos
4. **Validación adicional**: Verificar permisos en cada acción del controlador
5. **API de permisos**: Crear endpoints REST para gestión de permisos

## ?? Troubleshooting

**Problema**: El menú no aparece
- **Solución**: Verifica que el controlador tenga el atributo `[CargarPermisos]`

**Problema**: Todos los usuarios ven lo mismo
- **Solución**: Verifica que `Session["Rol"]` esté correctamente asignado en el Login

**Problema**: Error al acceder a Permisos
- **Solución**: Verifica que el usuario sea Administrador y tenga sesión activa
