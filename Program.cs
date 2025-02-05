
using System;
using System.Numerics;

class Program
{
    static void Main()
    {
        BackupManager manager = new BackupManager();

        // Ajout de quelques tâches de sauvegarde
        BackupJob job1 = new BackupJob(
                    "Sauvegarde Documents",
                    "C:\\Users\\axelc\\Desktop\\dev\\EasySave\\DataManipulation\\Source1",
                    "C:\\Users\\axelc\\Desktop\\dev\\EasySave\\DataManipulation\\Backup1",
                    BackupType.Complete
                );

        BackupJob job2 = new BackupJob(
            "Sauvegarde Images",
            "C:\\Users\\axelc\\Desktop\\dev\\EasySave\\DataManipulation\\Source2",
            "C:\\Users\\axelc\\Desktop\\dev\\EasySave\\DataManipulation\\Backup2",
            BackupType.Differential
        );



        manager.AddBackupJob(job1);
        manager.AddBackupJob(job2);

        // Exécution de toutes les sauvegardes
        manager.RunAllBackups();

        // Affichage du statut des sauvegardes
        manager.DisplayStatus();
    }
}


//C: \Users\axelc\Desktop\dev\EasySave\DataManipulation