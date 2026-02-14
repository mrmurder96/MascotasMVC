using System;
using System.IO;
using System.Web;

namespace Integrador.Helpers
{
    /// <summary>
    /// Helper para manejo de subida de imágenes
    /// </summary>
    public static class ImageHelper
    {
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private const int TamañoMaximoMB = 5;
        private const long TamañoMaximoBytes = TamañoMaximoMB * 1024 * 1024;

        /// <summary>
        /// Guarda una imagen subida en el servidor
        /// </summary>
        /// <param name="file">Archivo subido</param>
        /// <param name="carpeta">Carpeta donde se guardará (ej: "mascotas", "perdidas")</param>
        /// <returns>Nombre del archivo guardado o null si hay error</returns>
        public static string GuardarImagen(HttpPostedFileBase file, string carpeta)
        {
            try
            {
                // Validar que se haya subido un archivo
                if (file == null || file.ContentLength == 0)
                    return null;

                // Validar tamaño
                if (file.ContentLength > TamañoMaximoBytes)
                    throw new Exception($"El archivo es muy grande. Tamaño máximo: {TamañoMaximoMB}MB");

                // Validar extensión
                string extension = Path.GetExtension(file.FileName).ToLower();
                if (!Array.Exists(ExtensionesPermitidas, ext => ext == extension))
                    throw new Exception($"Formato no permitido. Formatos aceptados: {string.Join(", ", ExtensionesPermitidas)}");

                // Generar nombre único
                string nombreArchivo = $"{Guid.NewGuid()}{extension}";

                // Crear la ruta completa
                string rutaCarpeta = HttpContext.Current.Server.MapPath($"~/Content/uploads/{carpeta}");
                
                // Crear directorio si no existe
                if (!Directory.Exists(rutaCarpeta))
                    Directory.CreateDirectory(rutaCarpeta);

                // Guardar archivo
                string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);
                file.SaveAs(rutaCompleta);

                return nombreArchivo;
            }
            catch (Exception ex)
            {
                // Log del error (puedes implementar tu sistema de logs aquí)
                System.Diagnostics.Debug.WriteLine($"Error al guardar imagen: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Elimina una imagen del servidor
        /// </summary>
        /// <param name="nombreArchivo">Nombre del archivo a eliminar</param>
        /// <param name="carpeta">Carpeta donde está el archivo</param>
        public static void EliminarImagen(string nombreArchivo, string carpeta)
        {
            try
            {
                if (string.IsNullOrEmpty(nombreArchivo))
                    return;

                string rutaArchivo = HttpContext.Current.Server.MapPath($"~/Content/uploads/{carpeta}/{nombreArchivo}");
                
                if (File.Exists(rutaArchivo))
                    File.Delete(rutaArchivo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar imagen: {ex.Message}");
                // No lanzamos excepción para no interrumpir el flujo si falla la eliminación
            }
        }

        /// <summary>
        /// Valida si un archivo es una imagen válida
        /// </summary>
        public static bool EsImagenValida(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return false;

            if (file.ContentLength > TamañoMaximoBytes)
                return false;

            string extension = Path.GetExtension(file.FileName).ToLower();
            return Array.Exists(ExtensionesPermitidas, ext => ext == extension);
        }

        /// <summary>
        /// Obtiene la URL pública de una imagen
        /// </summary>
        public static string ObtenerUrlImagen(string nombreArchivo, string carpeta)
        {
            if (string.IsNullOrEmpty(nombreArchivo))
                return "/Content/images/no-image.png";

            return $"/Content/uploads/{carpeta}/{nombreArchivo}";
        }
    }
}
