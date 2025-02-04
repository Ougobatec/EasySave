using System;
using EasySave.DataManipulation;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Démarrage de l'application...");

            // Récupération du Singleton
            Singleton monSingleton = Singleton.Instance;

            // Abonnement à l'événement pour afficher les logs en temps réel
            monSingleton.LogEvent += (sender, log) =>
            {
                Console.WriteLine($"[EVENT] Log reçu: {log}");
            };

            // Tester les logs
            //monSingleton.MonLogger.Info("Démarrage du programme.");
            //monSingleton.MonLogger.Debug("Debugging en cours...");
            //monSingleton.MonLogger.Warning("Attention, potentiel problème.");
            monSingleton.MonLogger.Error("Erreur détectée !");
            monSingleton.MonLogger.Trace("Trace du processus.");

            //Console.WriteLine("Logs écrits avec succès !");


            DataManipulation.DataManipulation.Copy("source.txt", "dest.txt");
        }
    }
}
