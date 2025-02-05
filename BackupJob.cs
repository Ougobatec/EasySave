using System;
using System.IO;
using System.Collections.Generic;
using EasySave.Interfaces;
using EasySave.Enumerations;

namespace EasySave
{
    public class BackupJob
    {
        public string Name { get; set; }
        public string SourceDir { get; set; }
        public string TargetDir { get; set; }
        public BackupType Type { get; set; }
        public BackupStatus Status { get; set; }

        private List<IBackupObserver> BackupJob_Observers = new List<IBackupObserver>();

        public BackupJob(string name, string sourceDir, string targetDir, BackupType type)
        {
            Name = name;
            SourceDir = sourceDir;
            TargetDir = targetDir;
            Type = type;
            Status = BackupStatus.Inactive;
        }

        public void Attach(IBackupObserver observer)
        {
            BackupJob_Observers.Add(observer);
        }

        public void Detach(IBackupObserver observer)
        {
            BackupJob_Observers.Remove(observer);
        }

        public void RunBackup()
        {
            try
            {
                if (!Directory.Exists(SourceDir))
                {
                    Console.WriteLine($"Le répertoire source {SourceDir} n'existe pas.");
                    return;
                }

                if (!Directory.Exists(TargetDir))
                {
                    Directory.CreateDirectory(TargetDir);
                }

                string[] files = Directory.GetFiles(SourceDir);
                int totalFiles = files.Length;
                int remainingFiles = totalFiles;
                int remainingSize = files.Sum(file => (int)new FileInfo(file).Length); // Estimation de la taille restante

                Status = BackupStatus.InProgress;

                foreach (string sourceFile in files)
                {
                    string fileName = Path.GetFileName(sourceFile);
                    string destinationFile = Path.Combine(TargetDir, fileName);

                    try
                    {
                        DataManipulation.Copy(sourceFile, destinationFile);
                        remainingFiles--;
                        remainingSize -= (int)new FileInfo(sourceFile).Length;

                        NotifyObservers(sourceFile, destinationFile, remainingFiles, remainingSize);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors de la copie de {sourceFile}: {ex.Message}");
                    }
                }

                Status = BackupStatus.Completed;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution de la sauvegarde {Name}: {ex.Message}");
                Status = BackupStatus.Failed;
            }
        }

        private void NotifyObservers(string currentFile, string destinationFile, int remainingFiles, int remainingSize)
        {
            foreach (var observer in BackupJob_Observers)
            {
                observer.Update(Name, DateTime.Now, Status, remainingFiles, remainingSize, currentFile, destinationFile);
            }
        }
    }
}



