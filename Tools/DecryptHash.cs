using System;
using System.Configuration;
using Integrador.Helpers;

class Program
{
    static void Main()
    {
        Console.Write("ClaveHash (base64) a descifrar: ");
        var hash = Console.ReadLine()?.Trim();
        var claveAes = ConfigurationManager.AppSettings["ClaveAES"];
        if (string.IsNullOrEmpty(claveAes))
        {
            Console.WriteLine("ERROR: falta appSetting 'ClaveAES' en el config.");
            return;
        }

        try
        {
            var plain = Integrador.Helpers.AesEncryption.Decrypt(hash, claveAes);
            Console.WriteLine("Texto plano:");
            Console.WriteLine(plain);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al descifrar: " + ex.Message);
        }
    }
}