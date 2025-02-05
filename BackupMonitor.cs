public class BackupMonitor : IBackupObserver
{
    public void Update(string backupName, DateTime timestamp, BackupStatus status, int remainingFiles, long remainingSize, string currentFile, string destinationFile)
    {
    }
}