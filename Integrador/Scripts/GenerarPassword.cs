using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GenerarPassword
{
    class Program
    {
        static void Main(string[] args)
        {
            string claveAES = "IntegradorClaveAES2025";
            string password = "Admin123";
            
            string encrypted = Encrypt(password, claveAES);
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Cifrada: {encrypted}");
            
            // Verificar descifrando
            string decrypted = Decrypt(encrypted, claveAES);
            Console.WriteLine($"Verificación: {decrypted}");
        }
        
        static byte[] GetKey(string clave)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(clave));
            }
        }
        
        static string Encrypt(string textoPlano, string clave)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey(clave);
                aes.IV = new byte[16];

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(textoPlano);
                    sw.Close();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        
        static string Decrypt(string textoCifrado, string clave)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey(clave);
                aes.IV = new byte[16];

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(textoCifrado)))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
