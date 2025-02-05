using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        BackupManager manager = new BackupManager();

        // Ajout de quelques t�ches de sauvegarde
        BackupJob job1 = new BackupJob(
                    "Sauvegarde Documents",
                    ".\\..\\..\\..\\DataManipulation\\Source1",
                    ".\\..\\..\\..\\DataManipulation\\Backup1",
                    BackupType.Complete
                );

        BackupJob job2 = new BackupJob(
            "Sauvegarde Images",
            ".\\..\\..\\..\\DataManipulation\\Source2",
            ".\\..\\..\\..\\DataManipulation\\Backup2",
            BackupType.Differential
        );



        manager.AddBackupJob(job1);
        manager.AddBackupJob(job2);

        // Ex�cution de toutes les sauvegardes
        manager.RunAllBackups();

        // Affichage du statut des sauvegardes
        manager.DisplayStatus();
    }
}