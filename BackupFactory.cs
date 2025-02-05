public class BackupFactory
{
    public BackupJob CreateBackup(string name, string source, string target, BackupType type)
    {
        return new BackupJob(name, source, target, type);
    }
}

