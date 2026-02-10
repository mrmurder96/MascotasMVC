using System;
using System.Configuration;
using Integrador.Models;
using Integrador.Helpers;

namespace CreateAdminTool
{
    // Ejecuta esto como aplicación de consola desde una copia del proyecto (necesita acceso a la cadena de conexión).
    class Program
    {
        static void Main()
        {
            Console.Write("Email admin: ");
            var email = Console.ReadLine()?.Trim();

            Console.Write("Nombre: ");
            var nombres = Console.ReadLine()?.Trim();

            Console.Write("Apellidos: ");
            var apellidos = Console.ReadLine()?.Trim();

            Console.Write("Password (en claro): ");
            var password = ReadPassword();

            var claveAes = ConfigurationManager.AppSettings["ClaveAES"];
            if (string.IsNullOrEmpty(claveAes))
            {
                Console.WriteLine("ERROR: falta appSetting 'ClaveAES' en web.config.");
                return;
            }

            var claveHash = AesEncryption.Encrypt(password, claveAes);
            using (var db = new AdopcionMascotasEntities())
            {
                var u = new Usuarios
                {
                    Nombres = nombres,
                    Apellidos = apellidos,
                    Email = email,
                    ClaveHash = claveHash,
                    Salt = Guid.NewGuid().ToString(),
                    Rol = "Administrador",
                    FechaRegistro = DateTime.Now,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    EstaActivo = true
                };

                db.Usuarios.Add(u);
                db.SaveChanges();
                Console.WriteLine("Administrador creado. Id = " + u.Id);
            }
        }

        // Lee contraseña sin eco
        static string ReadPassword()
        {
            var pwd = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;
                if (key == ConsoleKey.Backspace && pwd.Length > 0)
                {
                    pwd = pwd.Substring(0, pwd.Length - 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    pwd += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();
            return pwd;
        }
    }
}