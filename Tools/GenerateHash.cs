using System;
using System.Configuration;
using Integrador.Helpers;

class Program
{
    static void Main()
    {
        Console.Write("Password en claro: ");
        var pwd = Console.ReadLine();
        var claveAes = ConfigurationManager.AppSettings["ClaveAES"];
        if (string.IsNullOrEmpty(claveAes))
        {
            Console.WriteLine("ERROR: falta appSetting 'ClaveAES' en web.config.");
            return;
        }
        var hash = AesEncryption.Encrypt(pwd, claveAes);
        Console.WriteLine("ClaveHash generada:");
        Console.WriteLine(hash);
    }
}