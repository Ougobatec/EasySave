using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using EasySave.Enumerations;
using EasySave.Views;
using Logger;
using EasySave.Modeles;

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
            try
            {
                //Console.WriteLine(HashFromFile(new FileInfo("fichier.txt")));
                ////afficher le hash d'un fichier

                //CLIView cli = new CLIView();
                //cli.Start();

                var logger = Logger<ModelLog>.GetInstance();

                logger.Log(new ModelLog
                {
                    Timestamp = DateTime.Now,
                    BackupName = "Backup1",
                    Source = "C:/source",
                    Destination = "C:/destination",
                    FileSize = 1000,
                    TransfertTime = new TimeSpan(0, 0, 10)
                });

                Console.WriteLine("Log entry created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
