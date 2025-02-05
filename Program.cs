using System;
using System.Numerics;
using EasySave.Enumerations;

namespace EasySave
{
    class Program
    {
        static void Main()
        {
            CLIInterface cli = new CLIInterface();
            cli.Start();
        }
    }

    class CLIInterface
    {
        private BackupManager manager = new BackupManager();

        public void Start()
        {
            while (true)
            {
                Console.WriteLine("1. Ajouter une sauvegarde");
                Console.WriteLine("2. Lancer toutes les sauvegardes");
                Console.WriteLine("3. Lancer une sauvegarde");
                Console.WriteLine("4. Afficher le statut");
                Console.WriteLine("5. Quitter");
                Console.Write("Choisissez une option: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        AddBackupJob();
                        break;
                    case "2":
                        manager.RunAllBackups();
                        break;
                    case "3":
                        manager.RunABackups();
                        break;
                    case "4":
                        manager.DisplayStatus();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }

        private void AddBackupJob()
        {
            Console.Write("Nom de la sauvegarde: ");
            string name = Console.ReadLine();
            Console.Write("Source: ");
            string source = Console.ReadLine();
            Console.Write("Destination: ");
            string destination = Console.ReadLine();
            Console.Write("Type (Complete/Differential): ");
            string type = Console.ReadLine();

            BackupType backupType = type.ToLower() == "complete" ? BackupType.Complete : BackupType.Differential;
            BackupJob job = new BackupJob(name, source, destination, backupType);
            manager.AddBackupJob(job);
            Console.WriteLine("Sauvegarde ajoutée.");
        }
    }
}
