using System;
using System.Collections.Generic;

namespace EasySave
{
    public class BackupManager
    {
        private List<BackupJob> BackupManager_BackupJobs = new List<BackupJob>();

        public void AddBackupJob(BackupJob job)
        {
            BackupManager_BackupJobs.Add(job);
        }

        public void RunAllBackups()
        {
            foreach (var job in BackupManager_BackupJobs)
            {
                job.RunBackup();
            }
        }
        public void RunABackups()
        {
            Console.WriteLine("Selectionnez la backup a executer parmis celle ci :");

            foreach (var job in BackupManager_BackupJobs)
            {
                Console.WriteLine($"- {job.Name}");
            }

            string choice = Console.ReadLine();

            var selectedJob = BackupManager_BackupJobs.FirstOrDefault(job =>
                string.Equals(job.Name, choice, StringComparison.OrdinalIgnoreCase));

            if (selectedJob != null)
            {
                selectedJob.RunBackup();
            }
            else
            {
                Console.WriteLine("Aucune sauvegarde correspondante trouvée.");
            }
        }



        public void DisplayStatus()
        {
            foreach (var job in BackupManager_BackupJobs)
            {
                Console.WriteLine($"Backup Job: {job.Name}, Status: {job.Status}");
            }
        }
    }
}
