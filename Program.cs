using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using EasySave.Enumerations;
using EasySave.Views;

namespace EasySave
{
    class Program
    {

        static string HashFromFile(FileInfo file)
        {
            byte[] firstHash = MD5.Create().ComputeHash(file.OpenRead());

            return Encoding.Default.GetString(firstHash);
        }

        //recuper le hash d'un fichier

        static void Main()
        {
            Console.WriteLine(HashFromFile(new FileInfo("fichier.txt")));
            //afficher le hash d'un fichier

            CLIView cli = new CLIView();
            cli.Start();
        }
    }
}
