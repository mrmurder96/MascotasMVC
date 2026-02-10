using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Integrador.Helpers
{
    public static class AesEncryption
    {
        private static byte[] GetKey(string clave)
        {
            if (string.IsNullOrEmpty(clave))
                throw new ArgumentException("La clave de encriptación es nula o vacía");

            // Asegura una clave de 32 bytes (AES-256)
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(clave));
            }
        }

        public static string Encrypt(string textoPlano, string clave)
        {
            if (string.IsNullOrEmpty(textoPlano))
                throw new ArgumentException("El texto a encriptar es nulo o vacío");

            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey(clave);
                aes.IV = new byte[16]; // IV fijo (para aprendizaje está bien)

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

        public static string Decrypt(string textoCifrado, string clave)
        {
            if (string.IsNullOrEmpty(textoCifrado))
                throw new ArgumentException("El texto cifrado es nulo o vacío");

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
