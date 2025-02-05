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
        // Logique de sauvegarde
    }

    private void NotifyObservers(string currentFile, string destinationFile, int remainingFiles, int remainingSize)
    {
        foreach (var observer in BackupJob_Observers)
        {
            observer.Update(Name, DateTime.Now, Status, remainingFiles, remainingSize, currentFile, destinationFile);
        }
    }
}

