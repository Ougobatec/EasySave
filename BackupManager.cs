using System;
using System.Collections.Generic;

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

    public void DisplayStatus()
    {
        foreach (var job in BackupManager_BackupJobs)
        {
            Console.WriteLine($"Backup Job: {job.Name}, Status: {job.Status}");
        }
    }
}