using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.DataManipulation
{
    internal class DataManipulation


    {
        public static void Copy(string sourceFileName, string destFileName)
        {
            Singleton monSingleton = Singleton.Instance;

            // Abonnement à l'événement pour afficher les logs en temps réel
            monSingleton.LogEvent += (sender, log) =>
            {
                Console.WriteLine($"[EVENT] Log reçu: {log}");
            };

            if (File.Exists(sourceFileName))
            {
                File.Copy(sourceFileName, destFileName, true);
                monSingleton.MonLogger.Info("fichier coppier");
            }
            else
            {
                throw new FileNotFoundException("Source file not found.", sourceFileName);


                // Tester les logs
                monSingleton.MonLogger.Error("errue pour la copie du ficher");
            }
        }
    }

}

