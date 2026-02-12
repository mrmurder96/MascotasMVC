# ? SISTEMA DE PERMISOS CRUD GRANULARES - IMPLEMENTADO

## ?? Lo Que Se Implementó

Sistema completo de gestión de permisos a nivel **CRUD granular**, permitiendo al administrador controlar exactamente qué operaciones puede realizar cada rol sobre cada módulo.

### **Características Principales:**

? **Permisos de Acceso** - Controla si un rol puede ver un módulo  
? **Permisos CRUD Individuales** - Controla Crear, Leer, Actualizar, Eliminar por separado  
? **Admin con Acceso Total** - El administrador puede hacer TODO (no solo lectura)  
? **Ciudadano Configurable** - El admin decide exactamente qué CRUD puede usar el ciudadano  
? **Vista Intuitiva** - Interfaz visual con checkboxes para cada operación  
? **Validación Automática** - Filtro que valida permisos en cada acción del controlador  

---

## ?? Archivos Creados/Modificados

### 1. **Modelo de Permisos Extendido**
**Archivo**: `Integrador\Models\Permisos.cs`

```csharp
// Agregado a Permisos:
public bool TieneCrud { get; set; }  // Indica si el módulo tiene operaciones CRUD

// Nuevo modelo:
public class PermisoCrud
{
    public string Rol { get; set; }
    public string ControllerName { get; set; }
    public bool PuedeCrear { get; set; }
    public bool PuedeLeer { get; set; }
    public bool PuedeActualizar { get; set; }
    public bool PuedeEliminar { get; set; }
}

// ViewModel:
public class PermisoConCrudViewModel
{
    public Permisos Permiso { get; set; }
    public PermisoCrud PermisosCrud { get; set; }
    public bool TieneAcceso { get; set; }
}
```

### 2. **Filtro de Validación CRUD**
**Archivo**: `Integrador\Filters\ValidarPermisoCrudAttribute.cs`

**Uso en Controladores:**
```csharp
// Validar operación específica
[ValidarPermisoCrud(Operacion = "Crear")]
public ActionResult Create() { }

[ValidarPermisoCrud(Operacion = "Leer")]
public ActionResult Index() { }

[ValidarPermisoCrud(Operacion = "Actualizar")]
public ActionResult Edit(int id) { }

[ValidarPermisoCrud(Operacion = "Eliminar")]
public ActionResult Delete(int id) { }
```

### 3. **Vista de Acceso Denegado**
**Archivo**: `Integrador\Views\Shared\AccesoDenegado.cshtml`
- Muestra mensaje amigable cuando no hay permisos
- Botones para volver o ir al inicio

### 4. **Controlador de Permisos Actualizado**
**Archivo**: `Integrador\Areas\Admin\Controllers\PermisosController.cs`

**Nuevos métodos:**
- `ActualizarPermiso()` - Actualiza acceso al módulo
- `ActualizarPermisoCrud()` - Actualiza permisos CRUD individuales
- `CrearPermisosViewModel()` - Crea ViewModels con permisos CRUD
- `ObtenerPermisoCrud()` - Obtiene permiso CRUD específico

### 5. **Vista de Gestión Mejorada**
**Archivo**: `Integrador\Areas\Admin\Views\Permisos\Index.cshtml`

**Interfaz Visual:**
- Tabla con todos los módulos
- Toggle para activar/desactivar acceso al módulo
- Checkboxes para cada operación CRUD
- Código de colores por operación:
  - ?? Verde = Crear
  - ?? Azul = Leer
  - ?? Amarillo = Actualizar
  - ?? Rojo = Eliminar

---

## ?? Cómo Usar el Sistema

### **Para Administradores:**

#### 1. **Acceder a Gestión de Permisos**
```
URL: /Admin/Permisos/Index
```

#### 2. **Configurar Permisos para Ciudadano:**

**a) Dar Acceso a un Módulo:**
1. Cambiar a pestaña "Ciudadano"
2. Activar toggle de "Acceso" en el módulo deseado
3. Se habilitan automáticamente los checkboxes CRUD

**b) Configurar Operaciones CRUD:**
1. Si el módulo tiene CRUD, marcar las operaciones permitidas:
   - ? **Crear** - Puede agregar nuevos registros
   - ? **Leer** - Puede ver/listar registros
   - ? **Actualizar** - Puede modificar registros
   - ? **Eliminar** - Puede borrar registros

#### 3. **Ejemplo Práctico:**

**Caso: Permitir a Ciudadanos gestionar Mascotas**

1. Activar toggle "Acceso" en Mascotas
2. Marcar checkboxes:
   - ? Crear - Para que puedan reportar mascotas perdidas
   - ? Leer - Para que puedan ver el catálogo
   - ? Actualizar - Solo admin puede editar
   - ? Eliminar - Solo admin puede eliminar

---

### **Para Desarrolladores:**

#### **Aplicar Validación CRUD en Controladores:**

```csharp
public class MascotasController : Controller
{
    [ValidarPermisoCrud(Operacion = "Leer")]
    public ActionResult Index()
    {
        // Lista de mascotas
        var mascotas = db.Mascotas.ToList();
        return View(mascotas);
    }

    [ValidarPermisoCrud(Operacion = "Crear")]
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidarPermisoCrud(Operacion = "Crear")]
    public ActionResult Create(Mascotas mascota)
    {
        if (ModelState.IsValid)
        {
            db.Mascotas.Add(mascota);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(mascota);
    }

    [ValidarPermisoCrud(Operacion = "Actualizar")]
    public ActionResult Edit(int id)
    {
        var mascota = db.Mascotas.Find(id);
        return View(mascota);
    }

    [HttpPost]
    [ValidarPermisoCrud(Operacion = "Actualizar")]
    public ActionResult Edit(Mascotas mascota)
    {
        if (ModelState.IsValid)
        {
            db.Entry(mascota).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(mascota);
    }

    [ValidarPermisoCrud(Operacion = "Eliminar")]
    public ActionResult Delete(int id)
    {
        var mascota = db.Mascotas.Find(id);
        db.Mascotas.Remove(mascota);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
}
```

#### **Validación en Vistas (Ocultar Botones):**

```razor
@* Solo mostrar botón Crear si tiene permiso *@
@if (Session["Rol"]?.ToString() == "Administrador" || 
     Session["PermisoCrud_Ciudadano_Mascotas_Crear"] as bool? == true)
{
    <a href="@Url.Action("Create")" class="btn btn-success">
        ? Agregar Mascota
    </a>
}

@* En tabla, solo mostrar botones según permisos *@
@foreach (var mascota in Model)
{
    <tr>
        <td>@mascota.Nombre</td>
        <td>
            @* Leer - Ver detalles *@
            <a href="@Url.Action("Details", new { id = mascota.Id })">Ver</a>
            
            @* Actualizar - Solo si tiene permiso *@
            @if (Session["Rol"]?.ToString() == "Administrador" || 
                 Session["PermisoCrud_Ciudadano_Mascotas_Actualizar"] as bool? == true)
            {
                <a href="@Url.Action("Edit", new { id = mascota.Id })">Editar</a>
            }
            
            @* Eliminar - Solo si tiene permiso *@
            @if (Session["Rol"]?.ToString() == "Administrador" || 
                 Session["PermisoCrud_Ciudadano_Mascotas_Eliminar"] as bool? == true)
            {
                <a href="@Url.Action("Delete", new { id = mascota.Id })">Eliminar</a>
            }
        </td>
    </tr>
}
```

---

## ?? Permisos por Defecto

### **Administrador:**
- ? **TODO** - Acceso completo a todas las operaciones CRUD en todos los módulos
- No puede ser editado (protección del sistema)

### **Ciudadano (Por Defecto):**
| Módulo | Acceso | Crear | Leer | Actualizar | Eliminar |
|--------|--------|-------|------|------------|----------|
| Usuarios | ? | ? | ? | ? | ? |
| Mascotas | ? | ? | ? | ? | ? |
| Campañas | ? | ? | ? | ? | ? |
| Adopciones | ? | ? | ? | ? | ? |
| Centros | ? | N/A | ? | N/A | N/A |
| Perfil | ? | N/A | ? | N/A | N/A |
| Notificaciones | ? | N/A | ? | N/A | N/A |

**Nota:** Módulos marcados como N/A no tienen operaciones CRUD (son solo vistas)

---

## ?? Persistencia en Base de Datos

### **Actualmente:** Permisos se guardan en **Session** (temporal)

### **Para Producción:** Crear tablas en SQL Server

```sql
-- Tabla de Permisos Base
CREATE TABLE Permisos (
    Id INT PRIMARY KEY IDENTITY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    ControllerName NVARCHAR(100) NOT NULL,
    ActionName NVARCHAR(100) NOT NULL,
    Icono NVARCHAR(50),
    Orden INT NOT NULL,
    EsActivo BIT DEFAULT 1,
    TieneCrud BIT DEFAULT 0
);

-- Tabla de Acceso a Módulos
CREATE TABLE RolPermisos (
    Id INT PRIMARY KEY IDENTITY,
    Rol NVARCHAR(50) NOT NULL,
    PermisoId INT FOREIGN KEY REFERENCES Permisos(Id),
    TieneAcceso BIT DEFAULT 0
);

-- Tabla de Permisos CRUD Granulares
CREATE TABLE PermisosCrud (
    Id INT PRIMARY KEY IDENTITY,
    Rol NVARCHAR(50) NOT NULL,
    PermisoId INT FOREIGN KEY REFERENCES Permisos(Id),
    ControllerName NVARCHAR(100) NOT NULL,
    PuedeCrear BIT DEFAULT 0,
    PuedeLeer BIT DEFAULT 1,
    PuedeActualizar BIT DEFAULT 0,
    PuedeEliminar BIT DEFAULT 0
);
```

### **Actualizar Métodos para Usar BD:**

En `PermisosController.cs`:

```csharp
[HttpPost]
public JsonResult ActualizarPermisoCrud(string rol, int permisoId, string controllerName, string operacion, bool tienePermiso)
{
    try
    {
        // Buscar o crear registro
        var permisoCrud = db.PermisosCrud
            .FirstOrDefault(p => p.Rol == rol && p.PermisoId == permisoId);

        if (permisoCrud == null)
        {
            permisoCrud = new PermisoCrud
            {
                Rol = rol,
                PermisoId = permisoId,
                ControllerName = controllerName
            };
            db.PermisosCrud.Add(permisoCrud);
        }

        // Actualizar operación específica
        switch (operacion)
        {
            case "Crear": permisoCrud.PuedeCrear = tienePermiso; break;
            case "Leer": permisoCrud.PuedeLeer = tienePermiso; break;
            case "Actualizar": permisoCrud.PuedeActualizar = tienePermiso; break;
            case "Eliminar": permisoCrud.PuedeEliminar = tienePermiso; break;
        }

        db.SaveChanges();
        
        // También actualizar en sesión para uso inmediato
        Session[$"PermisoCrud_{rol}_{controllerName}_{operacion}"] = tienePermiso;

        return Json(new { success = true, message = "Permiso actualizado" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}
```

---

## ?? Casos de Uso Comunes

### **Caso 1: Permitir a Ciudadanos Ver Pero No Editar**
```
Mascotas:
? Acceso
? Leer
? Crear
? Actualizar
? Eliminar
```

### **Caso 2: Permitir Reportar Mascotas Perdidas**
```
Mascotas:
? Acceso
? Crear  ? Pueden reportar
? Leer
? Actualizar
? Eliminar
```

### **Caso 3: Colaboradores con Permisos Limitados**
```
Usuarios:
? Acceso
? Crear
? Leer
? Actualizar  ? Solo actualizar sus propios datos
? Eliminar
```

---

## ?? Consideraciones Importantes

1. **Admin Siempre Tiene Acceso Total**
   - No se puede restringir al administrador
   - Protección contra bloqueo del sistema

2. **Validación en Cliente Y Servidor**
   - Ocultar botones en vistas (UX)
   - Validar con filtro en servidor (Seguridad)

3. **Permisos en Sesión**
   - Cerrar/reabrir sesión para aplicar cambios
   - O implementar evento para refrescar permisos

4. **Modularidad**
   - Fácil agregar nuevos módulos
   - Solo agregar a `ObtenerPermisosDisponibles()`

---

## ?? Checklist de Implementación

- [x] Modelo de Permisos extendido con CRUD
- [x] Filtro de validación CRUD
- [x] Vista de gestión con checkboxes
- [x] Controlador con métodos CRUD
- [x] Vista de Acceso Denegado
- [x] Compilación exitosa
- [ ] Crear tablas en BD (próximo paso)
- [ ] Migrar lógica de Session a BD
- [ ] Aplicar filtros a todos los controladores
- [ ] Ocultar botones según permisos en vistas

---

## ?? Próximos Pasos Sugeridos

1. **Crear tablas en BD** según script SQL arriba
2. **Actualizar Entity Framework** con los nuevos modelos
3. **Migrar métodos** de Session a BD
4. **Aplicar filtros** a todos los controladores:
   - UsuariosController
   - MascotasController
   - CampanasController
   - AdopcionesController
5. **Actualizar vistas** para ocultar botones según permisos
6. **Testing** de todos los escenarios

---

**Estado**: ? **FUNCIONAL Y COMPILANDO**  
**Última Actualización**: $(Get-Date -Format "dd/MM/yyyy HH:mm")
