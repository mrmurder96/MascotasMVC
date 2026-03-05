-- =============================================
-- Script: Agregar Permiso de Campañas para Ciudadanos
-- Descripción: Agrega el permiso de ver Campañas en el área de usuario (Ciudadano)
-- Fecha: 2024
-- =============================================

-- Verificar si ya existe el permiso de Campañas
DECLARE @PermisoId INT;
SELECT @PermisoId = Id FROM Permisos WHERE ControllerName = 'Campanas' AND ActionName = 'Index';

-- Si no existe el permiso, crearlo
IF @PermisoId IS NULL
BEGIN
    INSERT INTO Permisos (Nombre, ControllerName, ActionName, Icono, Orden, TieneCrud, Descripcion, EstaActivo, FechaCreacion)
    VALUES (
        'Campañas',           -- Nombre del menú
        'Campanas',           -- Nombre del controlador
        'Index',              -- Acción por defecto
        'fa-bullhorn',        -- Icono Font Awesome
        50,                   -- Orden (alto para que aparezca después de otros menús)
        0,                    -- TieneCrud = false (solo lectura para ciudadanos)
        'Ver campañas de adopción y eventos',
        1,                    -- EstaActivo = true
        GETDATE()
    );
    
    SET @PermisoId = SCOPE_IDENTITY();
    PRINT 'Permiso de Campañas creado con ID: ' + CAST(@PermisoId AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'El permiso de Campañas ya existe con ID: ' + CAST(@PermisoId AS VARCHAR(10));
END

-- Verificar si el rol Ciudadano ya tiene el permiso asignado
IF NOT EXISTS (SELECT 1 FROM RolPermisos WHERE Rol = 'Ciudadano' AND PermisoId = @PermisoId)
BEGIN
    INSERT INTO RolPermisos (Rol, PermisoId, TieneAcceso, PuedeCrear, PuedeLeer, PuedeActualizar, PuedeEliminar, FechaAsignacion)
    VALUES (
        'Ciudadano',          -- Rol
        @PermisoId,           -- PermisoId
        1,                    -- TieneAcceso = true
        0,                    -- PuedeCrear = false
        1,                    -- PuedeLeer = true
        0,                    -- PuedeActualizar = false
        0,                    -- PuedeEliminar = false
        GETDATE()
    );
    
    PRINT 'Permiso de Campañas asignado al rol Ciudadano exitosamente.';
END
ELSE
BEGIN
    -- Actualizar el permiso existente para asegurar que tiene acceso
    UPDATE RolPermisos 
    SET TieneAcceso = 1, PuedeLeer = 1, FechaAsignacion = GETDATE()
    WHERE Rol = 'Ciudadano' AND PermisoId = @PermisoId;
    
    PRINT 'El rol Ciudadano ya tenía el permiso de Campañas. Se actualizó el acceso.';
END

-- Verificar el resultado
SELECT 
    p.Id AS PermisoId,
    p.Nombre,
    p.ControllerName,
    rp.Rol,
    rp.TieneAcceso,
    rp.PuedeLeer
FROM Permisos p
INNER JOIN RolPermisos rp ON p.Id = rp.PermisoId
WHERE p.ControllerName = 'Campanas' AND rp.Rol = 'Ciudadano';

PRINT 'Script completado exitosamente.';
